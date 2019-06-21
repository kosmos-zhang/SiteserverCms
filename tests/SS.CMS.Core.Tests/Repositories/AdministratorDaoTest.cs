﻿using System;
using SS.CMS.Enums;
using SS.CMS.Models;
using SS.CMS.Utils.Tests;
using Xunit;
using Xunit.Abstractions;

namespace SS.CMS.Core.Tests.Repositories
{
    [Collection("Database collection")]
    public class UserRepositoryTest
    {
        private readonly DatabaseFixture _fixture;
        private readonly ITestOutputHelper _output;

        private const string TestUserName = "Tests_UserName";

        public UserRepositoryTest(DatabaseFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [SkippableFact]
        public void TestInsert()
        {
            Skip.IfNot(TestEnv.IsTestMachine);

            var userInfo = new UserInfo();
            var id = _fixture.UserRepository.Insert(userInfo, out _);

            Assert.True(id == 0);

            userInfo = new UserInfo
            {
                UserName = TestUserName,
                Password = "InsertTest"
            };

            id = _fixture.UserRepository.Insert(userInfo, out var errorMessage);
            _output.WriteLine(errorMessage);

            Assert.True(id == 0);

            userInfo = new UserInfo
            {
                UserName = TestUserName,
                Password = "InsertTest@2"
            };

            id = _fixture.UserRepository.Insert(userInfo, out errorMessage);
            _output.WriteLine(errorMessage);

            Assert.True(id > 0);
            Assert.True(!string.IsNullOrWhiteSpace(userInfo.Password));
            Assert.True(userInfo.PasswordFormat == PasswordFormat.Encrypted.Value);
            Assert.True(!string.IsNullOrWhiteSpace(userInfo.PasswordSalt));

            userInfo = _fixture.UserRepository.GetByUserName(TestUserName);

            var password = userInfo.Password;
            var passwordFormat = userInfo.PasswordFormat;
            var passwordSalt = userInfo.PasswordSalt;

            userInfo.Password = "cccc@d";

            var updated = _fixture.UserRepository.Update(userInfo, out _);
            Assert.True(updated);
            Assert.True(userInfo.Password == password);
            Assert.True(userInfo.PasswordFormat == passwordFormat);
            Assert.True(userInfo.PasswordSalt == passwordSalt);

            userInfo = _fixture.UserRepository.GetByUserName(TestUserName);
            Assert.NotNull(userInfo);
            Assert.Equal(TestUserName, userInfo.UserName);

            var countOfFailedLogin = userInfo.CountOfFailedLogin;

            updated = _fixture.UserRepository.UpdateLastActivityDateAndCountOfFailedLogin(userInfo);
            Assert.True(updated);
            Assert.Equal(countOfFailedLogin, userInfo.CountOfFailedLogin - 1);

            userInfo = _fixture.UserRepository.GetByUserName(TestUserName);
            Assert.NotNull(userInfo);
            Assert.Equal(TestUserName, userInfo.UserName);

            var deleted = _fixture.UserRepository.Delete(userInfo);

            Assert.True(deleted);
        }
    }
}
