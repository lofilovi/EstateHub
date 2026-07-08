using EstateHub_code.Services;

namespace EstateHub_code.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_IsDeterministic_ForSameInput()
        {
            var first = PasswordHasher.Hash("admin123");
            var second = PasswordHasher.Hash("admin123");

            Assert.Equal(first, second);
        }

        [Fact]
        public void Hash_ProducesDifferentOutput_ForDifferentInput()
        {
            var hash1 = PasswordHasher.Hash("admin123");
            var hash2 = PasswordHasher.Hash("admin124");

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Hash_ReturnsUppercase64CharacterHex()
        {
            var hash = PasswordHasher.Hash("admin123");

            Assert.Equal(64, hash.Length);
            Assert.Equal(hash.ToUpperInvariant(), hash);
            Assert.Matches("^[0-9A-F]{64}$", hash);
        }

        [Fact]
        public void Hash_NeverReturnsPlainTextPassword()
        {
            var password = "admin123";

            var hash = PasswordHasher.Hash(password);

            Assert.DoesNotContain(password, hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
