using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TwittorApp.Data;
using TwittorApp.Dtos;
using TwittorApp.Helpers;
using TwittorApp.Kafka;
using TwittorApp.Models;

namespace TwittorApp.GraphQL
{
    public class Mutation
    {
        //Administration

        public async Task<TransactionStatus> RegisterUserAsync (
            RegisterUser input,
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User Already Exist"));
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
                IsBanned = false,
                UserCreated = DateTime.Now
            };

            var key = "user-createing-" + DateTime.Now.ToString();
            var val = JObject.FromObject(newUser).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "user-add", key, val);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "Registry User Success");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        public async Task<UserToken> LoginAsync (
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings,
            [Service] IOptions<KafkaSettings> kafkaSettings,
            [Service] ApplicationDbContext context)
        {
        {
            var user = context.Users.Where(o => o.Username == input.Username).SingleOrDefault();
            if(user == null)
            {
                return await Task.FromResult(new UserToken(null,null,"Invalid username or password"));
            }
            bool passwordValid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if(passwordValid)
            {
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));
                var userRoles = context.UserRoles.Where(o => o.UserId == user.UserId).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.RoleId == userRole.RoleId).FirstOrDefault();
                    if(role!=null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }
                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,   
                    claims: claims,
                    signingCredentials: credentials
                );

                var key = "user-login-" + DateTime.Now.ToString();
                var val = JObject.FromObject(new { Message = "GraphQL Mutation User Login" }).ToString(Formatting.None);

                _ = await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

                var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                return await Task.FromResult(new UserToken(token, expired.ToString(), null));
            }
            else return await Task.FromResult(new UserToken(null,null,"Invalid username or password"));
        }
        }

        //Role

        public async Task<TransactionStatus> CreateRoleAsync (
            CreateNewRole input,
            [Service] ApplicationDbContext context, 
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var role = context.Roles.Where(o => o.Name == input.RoleName).SingleOrDefault();
            if (role != null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Role Already Exist"));
            }
            var newRole = new Role
            {
                Name = input.RoleName
            };

            var key = "role-createing-" + DateTime.Now.ToString();
            var val = JObject.FromObject(newRole).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "role-add", key, val);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        //User

        [Authorize(Roles = new[] { "Administrator", "Member" })]
        public async Task<TransactionStatus> UpdateUserAsync (
            UpdateUser input,
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = new User();
            user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Username Not Exist"));
            }

            user.FullName = input.FullName;
            user.Email = input.Email;
            user.Username = input.UserName;

            var key = "user-updating-" + DateTime.Now.ToString();
            var val = JObject.FromObject(user).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "user-update", key, val);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "Administrator", "Member" })]
        public async Task<TransactionStatus> UpdateUserPasswordAsync(
            UpdateUserPassword input,
            [Service] ApplicationDbContext context, 
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = new User();
            user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Username Not Exist"));
            }

            var valid = BCrypt.Net.BCrypt.Verify(input.OldPassword, user.Password);
            if (valid)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);
            }

            var key = "user-updating-password-" + DateTime.Now.ToString();
            var val = JObject.FromObject(user).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "user-update-password", key, val);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "Administrator" })]
        public async Task<TransactionStatus> BannedUserAsync(
            BannedUser input,
            [Service] ApplicationDbContext context, 
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = new User();
            user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Username Not Exist"));
            }

            user.IsBanned = input.IsBanned;

            var key = "user-banning-" + DateTime.Now.ToString();
            var val = JObject.FromObject(user).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "user-banned", key, val);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        //User Role

        public async Task<TransactionStatus> AssignUserToRoleAsync(
            AssignUserToRole input,
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).SingleOrDefault();
            var role = context.Roles.Where(o => o.Name == input.RoleName).SingleOrDefault();
            if (user == null || role == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User Or Role Not Found"));
            }

            var assignUserRole = context.UserRoles.Where(o => o.UserId == user.UserId && o.RoleId == role.RoleId).SingleOrDefault();
            if (assignUserRole != null)
            {
                return await Task.FromResult(new TransactionStatus(false, "UserRole already exist"));
            }
            var newUserRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId
            };

            var key = "user-role-createing-" + DateTime.Now.ToString();
            var val = JObject.FromObject(newUserRole).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "user-role-add", key, val);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        //Twittor App

        [Authorize(Roles = new[] { "Member" })]
        public async Task<TransactionStatus> CreateTwittorContentAsync(
            CreateContentTwittor input,
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(o => o.UserId == input.UserId && o.IsBanned.Equals(false)).SingleOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User Is Not Exist Or Banned"));
            }

                var newTwittor = new Twittor
            {
                TwittorContent = input.TwittorContent,
                TwittorCreated = DateTime.Now,
                UserId = user.UserId
            };

            var key = "twittor-content-createing-" + DateTime.Now.ToString();
            var val = JObject.FromObject(newTwittor).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "twittor-content-add", key, val);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        [Authorize(Roles = new[] { "Member"})]
        public async Task<TransactionStatus> DeleteTwittorContentAsync(
            DeleteTwittorContent input,
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {

            var user = context.Users.Where(o => o.UserId == input.UserId && o.IsBanned.Equals(false)).SingleOrDefault();

            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User Is Not Exist Or Banned"));
            }

            var twittor = context.Twittors.Where(o => o.TwittorId == input.TwittorId).SingleOrDefault();

            if (twittor == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Twittor Is Not Exist"));
            }

            var key = "twittor-content-deleting-" + DateTime.Now.ToString();
            var val = JObject.FromObject(twittor).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "twittor-content-delete", key, val);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }

        //Comment Twittor

        [Authorize(Roles = new[] { "Member" })]
        public async Task<TransactionStatus> CreateCommentTwittorAsync(
            CreateCommentTwittor input,
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(o => o.UserId == input.UserId && o.IsBanned.Equals(false)).SingleOrDefault();

            var twittor = context.Twittors.Where(o => o.TwittorId == input.TwittorId).SingleOrDefault();

            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User Is Not Exist Or Banned"));
            }

            if (twittor == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Twittor Is Not Exist"));
            }

            var newComment = new Comment
            {
                CommentContent = input.CommentContent,
                CommentCreated = DateTime.Now,
                TwittorId = twittor.TwittorId
            };

            var key = "comment-content-adding-" + DateTime.Now.ToString();
            var val = JObject.FromObject(newComment).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "comment-content-add", key, val);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }
    }
}
