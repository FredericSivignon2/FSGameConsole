using FSCPU;
using FluentAssertions;

namespace FSCPUTests
{
    /// <summary>
    /// Tests unitaires pour la classe StatusRegister du processeur 8 bits
    /// </summary>
    public class StatusRegisterTests
    {
        [Fact]
        public void StatusRegister_ShouldInitializeWithZeroValue()
        {
            // Arrange & Act
            var sr = new StatusRegister();

            // Assert
            sr.Value.Should().Be(0);
            sr.Zero.Should().BeFalse();
            sr.Carry.Should().BeFalse();
            sr.Overflow.Should().BeFalse();
            sr.Negative.Should().BeFalse();
        }

        [Fact]
        public void Reset_ShouldClearAllFlags()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.Value = 0xFF; // Tous les bits à 1

            // Act
            sr.Reset();

            // Assert
            sr.Value.Should().Be(0);
            sr.Zero.Should().BeFalse();
            sr.Carry.Should().BeFalse();
            sr.Overflow.Should().BeFalse();
            sr.Negative.Should().BeFalse();
        }

        [Fact]
        public void UpdateZeroFlag_ShouldSetFlagWhenValueIsZero()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.UpdateZeroFlag(0);

            // Assert
            sr.Zero.Should().BeTrue();
            sr.Value.Should().Be(0x01); // Bit 0 à 1
        }

        [Fact]
        public void UpdateZeroFlag_ShouldClearFlagWhenValueIsNotZero()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.Value = 0x01; // Zero flag initialement à 1

            // Act
            sr.UpdateZeroFlag(42);

            // Assert
            sr.Zero.Should().BeFalse();
            sr.Value.Should().Be(0x00);
        }

        [Fact]
        public void SetCarryFlag_ShouldSetBit1()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.SetCarryFlag(true);

            // Assert
            sr.Carry.Should().BeTrue();
            sr.Value.Should().Be(0x02); // Bit 1 à 1
        }

        [Fact]
        public void SetCarryFlag_ShouldClearBit1()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.Value = 0x02; // Carry flag initialement à 1

            // Act
            sr.SetCarryFlag(false);

            // Assert
            sr.Carry.Should().BeFalse();
            sr.Value.Should().Be(0x00);
        }

        [Fact]
        public void SetOverflowFlag_ShouldSetBit2()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.SetOverflowFlag(true);

            // Assert
            sr.Overflow.Should().BeTrue();
            sr.Value.Should().Be(0x04); // Bit 2 à 1
        }

        [Fact]
        public void UpdateNegativeFlag_ShouldSetFlagWhenBit7IsSet()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.UpdateNegativeFlag(0x80); // Bit 7 à 1

            // Assert
            sr.Negative.Should().BeTrue();
            sr.Value.Should().Be(0x08); // Bit 3 à 1
        }

        [Fact]
        public void UpdateNegativeFlag_ShouldClearFlagWhenBit7IsNotSet()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.Value = 0x08; // Negative flag initialement à 1

            // Act
            sr.UpdateNegativeFlag(0x7F); // Bit 7 à 0

            // Assert
            sr.Negative.Should().BeFalse();
            sr.Value.Should().Be(0x00);
        }

        [Fact]
        public void UpdateFlags_ShouldUpdateMultipleFlagsBasedOnResult()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act - Résultat zéro
            sr.UpdateFlags(0);

            // Assert
            sr.Zero.Should().BeTrue();
            sr.Carry.Should().BeFalse();
            sr.Negative.Should().BeFalse();
        }

        [Fact]
        public void UpdateFlags_ShouldSetCarryFlagForOverflow()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act - Résultat > 255
            sr.UpdateFlags(256);

            // Assert
            sr.Zero.Should().BeTrue(); // 256 & 0xFF = 0
            sr.Carry.Should().BeTrue(); // 256 > 0xFF
            sr.Negative.Should().BeFalse();
        }

        [Fact]
        public void UpdateFlags_ShouldSetNegativeFlagForNegativeResult()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act - Résultat avec bit 7 à 1
            sr.UpdateFlags(0x80);

            // Assert
            sr.Zero.Should().BeFalse();
            sr.Carry.Should().BeFalse();
            sr.Negative.Should().BeTrue();
        }

        [Fact]
        public void UpdateFlags_ShouldSetCarryFlagForNegativeResult()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act - Résultat négatif
            sr.UpdateFlags(-1);

            // Assert
            sr.Zero.Should().BeFalse();
            sr.Carry.Should().BeTrue(); // Résultat < 0
            sr.Negative.Should().BeTrue(); // (-1) & 0xFF = 0xFF, bit 7 = 1
        }

        [Fact]
        public void FlagsProperties_ShouldReflectCorrectBits()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act - Définir tous les flags
            sr.Value = 0x0F; // 00001111 en binaire

            // Assert
            sr.Zero.Should().BeTrue();     // Bit 0
            sr.Carry.Should().BeTrue();    // Bit 1
            sr.Overflow.Should().BeTrue(); // Bit 2
            sr.Negative.Should().BeTrue(); // Bit 3
        }

        [Fact]
        public void MultipleFlagsCanBeSetSimultaneously()
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.UpdateZeroFlag(0);     // Set Zero
            sr.SetCarryFlag(true);    // Set Carry
            sr.SetOverflowFlag(true); // Set Overflow

            // Assert
            sr.Zero.Should().BeTrue();
            sr.Carry.Should().BeTrue();
            sr.Overflow.Should().BeTrue();
            sr.Negative.Should().BeFalse();
            sr.Value.Should().Be(0x07); // 00000111 en binaire
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var sr = new StatusRegister();
            sr.UpdateZeroFlag(0);     // Z=1
            sr.SetCarryFlag(true);    // C=1
            sr.SetOverflowFlag(false); // O=0
            sr.UpdateNegativeFlag(0x80); // N=1

            // Act
            string result = sr.ToString();

            // Assert
            result.Should().Be("Z:1 C:1 O:0 N:1");
        }

        [Theory]
        [InlineData(0x00, true, false)]   // Zéro -> Zero=true, Negative=false
        [InlineData(0x7F, false, false)]  // Positif -> Zero=false, Negative=false
        [InlineData(0x80, false, true)]   // Bit 7 à 1 -> Zero=false, Negative=true
        [InlineData(0xFF, false, true)]   // Tous bits à 1 -> Zero=false, Negative=true
        public void UpdateZeroAndNegativeFlags_ShouldWorkCorrectly(byte value, bool expectedZero, bool expectedNegative)
        {
            // Arrange
            var sr = new StatusRegister();

            // Act
            sr.UpdateZeroFlag(value);
            sr.UpdateNegativeFlag(value);

            // Assert
            sr.Zero.Should().Be(expectedZero);
            sr.Negative.Should().Be(expectedNegative);
        }
    }
}