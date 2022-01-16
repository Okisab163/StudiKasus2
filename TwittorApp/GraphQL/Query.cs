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
    public class Query
    {
        public async Task<IQueryable<UserData>> GetUsersAsync(
            [Service] ApplicationDbContext context, 
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var key = "get-user-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GraphQL Query Get Users" }).ToString(Formatting.None);

            _ = await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var showProfile = context.Users.Select(p => new UserData()
            {
                UserId = p.UserId,
                FullName = p.FullName,
                Email = p.Email,
                Username = p.Username
            });

            return showProfile;
        }

        public async Task<IQueryable<UserData>> GetUsersByIdAsync(
            ViewUserById input,
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var key = "get-user-byid-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GraphQL Query Get Users By Id" }).ToString(Formatting.None);

            _ = await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var showProfile = context.Users.Select(p => new UserData()
            {
                UserId = p.UserId,
                FullName = p.FullName,
                Email = p.Email,
                Username = p.Username
            }).Where(p => p.UserId == input.UserId);

            return showProfile;
        }

        public async Task<IQueryable<Comment>> GetCommentsByTwittorIdAsync(
            ViewTwittorById input,
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {

            var comment = context.Comments.Where(o => o.TwittorId == input.TwittorId);

            var key = "get-comment-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GraphQL Query Get Comments" }).ToString(Formatting.None);

            _ = await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            return comment;
        }

        public async Task<IQueryable<Twittor>> GetTwittorAsync(
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {

            var twittor = context.Twittors;

            var key = "get-twittor-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GraphQL Query Get Twittor" }).ToString(Formatting.None);

            _ = await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            return twittor;
        }

        public async Task<IQueryable<Role>> GetRolesAsync(
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {

            var role = context.Roles;

            var key = "get-role-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GraphQL Query Get Role" }).ToString(Formatting.None);

            _ = await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            return role;
        }

        public async Task<IQueryable<UserRole>> GetUserRoleAsync(
            [Service] ApplicationDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {

            var userrole = context.UserRoles;

            var key = "get-userrole-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GraphQL Query Get User Role" }).ToString(Formatting.None);

            _ = await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            return userrole;
        }

    }
}
