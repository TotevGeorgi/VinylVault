using Xunit;
using CoreLayer;

namespace UnitTest
{
    public class PasswordHasherTests
    {
        private readonly IPasswordHasher _passwordHasher;

        public PasswordHasherTests()
        {
            _passwordHasher = new PasswordHasher();
        }

        [Fact]
        public void Hash_ThenVerify_ReturnsTrue()
        {
            var pw = "P@ssw0rd!";
            var hash = _passwordHasher.Hash(pw);

            Assert.True(_passwordHasher.Verify(pw, hash));
        }

        [Fact]
        public void Verify_WithWrongPassword_ReturnsFalse()
        {
            var hash = _passwordHasher.Hash("CorrectHorseBatteryStaple");

            Assert.False(_passwordHasher.Verify("wrong", hash));
        }
    }
}
