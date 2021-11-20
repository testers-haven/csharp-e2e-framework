﻿using FluentAssertions;
using TestingFramework.ApiClient.Endpoints;
using TestingFramework.ApiClient.Requests;
using TestingFramework.ApiClient.Responses;
using TestingFramework.Reporter;
using NUnit.Framework;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace TestingFramework
{
    [Parallelizable(scope: ParallelScope.All)]
    public class CommentTests
    {
        private UserData commentUser;
        private PostData post;

        [OneTimeTearDown]
        public void CloseAll()
        {
            ExtentManager.Reporter.Flush();
        }

        [TearDown]
        public void AfterTest()
        {
            ReportManager.EndTest();
        }

        [SetUp]
        public void Setup()
        {
            ReportManager.CreateTest(TestContext.CurrentContext.Test.ClassName, TestContext.CurrentContext.Test.Name);
            if (commentUser == null) GetRandomUser();
            if (post == null) GetRandomPost();
        }

        [Test]
        public void CreateCommentOnPost()
        {
            var request = new PostCommentRequest()
            {
                Name = commentUser.name,
                Email = commentUser.email,
                Body = "This is a test body"
            };

            var postResponse = CommentEndpoint.PostComment(request, post.id).Result;

            ReportManager.SetStepStatusPass("Response Code is: " + postResponse.StatusCode);
            ReportManager.SetStepStatusPass("Response Content is: " + postResponse.Content);

            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            postResponse.Content.Should().Contain(request.Name);
            postResponse.Content.Should().Contain(request.Email);
            postResponse.Content.Should().Contain(request.Body);
        }

        [Test]
        public void CreateCommentWithoutName()
        {
            var request = new PostCommentRequest()
            {
                Name = null,
                Email = commentUser.email,
                Body = "This is a test body"
            };

            var postResponse = CommentEndpoint.PostComment(request, post.id).Result;

            ReportManager.SetStepStatusPass("Response Code is: " + postResponse.StatusCode);
            ReportManager.SetStepStatusPass("Response Content is: " + postResponse.Content);


            postResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            postResponse.Content.Should().Contain("{\"field\":\"name\",\"message\":\"can't be blank\"}");
        }

        [Test]
        public void CreateCommentWithoutEmail()
        {
            var request = new PostCommentRequest()
            {
                Name = commentUser.name,
                Email = null,
                Body = "This is a test body"
            };

            var postResponse = CommentEndpoint.PostComment(request, post.id).Result;

            ReportManager.SetStepStatusPass("Response Code is: " + postResponse.StatusCode);
            ReportManager.SetStepStatusPass("Response Content is: " + postResponse.Content);

            postResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            postResponse.Content.Should().Contain("{\"field\":\"email\",\"message\":\"can't be blank\"}");
        }

        [Test]
        public void CreateCommentWithoutBody()
        {
            var request = new PostCommentRequest()
            {
                Name = commentUser.name,
                Email = commentUser.email,
                Body = null
            };

            var postResponse = CommentEndpoint.PostComment(request, post.id).Result;

            ReportManager.SetStepStatusPass("Response Code is: " + postResponse.StatusCode);
            ReportManager.SetStepStatusPass("Response Content is: " + postResponse.Content);

            postResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            postResponse.Content.Should().Contain("{\"field\":\"body\",\"message\":\"can't be blank\"}");
        }

        [Test]
        public void CreateCommentWithoutPostId()
        {
            var request = new PostCommentRequest()
            {
                Name = commentUser.name,
                Email = commentUser.email,
                Body = "This is a test body"
            };

            var postResponse = CommentEndpoint.PostComment(request, -1).Result;

            ReportManager.SetStepStatusPass("Response Code is: " + postResponse.StatusCode);
            ReportManager.SetStepStatusPass("Response Content is: " + postResponse.Content);

            postResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            postResponse.Content.Should().Contain("{\"field\":\"post\",\"message\":\"must exist\"}");
        }

        private void GetRandomUser()
        {
            ReportManager.SetStepStatusPass("Picking a random user");
            var response = UserEndpoint.GetActiveUsers().Result;
            var users = JsonSerializer.Deserialize<GetUsersResponse>(response.Content);
            commentUser = users.data.Take(1).First();
        }

        private void GetRandomPost()
        {
            ReportManager.SetStepStatusPass("Picking a random post");
            var response = PostEndpoint.GetPosts();
            var users = JsonSerializer.Deserialize<GetPostsResponse>(response.Result.Content);
            post = users.data.Take(1).First();
        }
    }
}
