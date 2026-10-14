using CoFast.Dispatch;

namespace CoFast.Tests;

public class HotkeyParserTests
{
    [Fact]
    public void CtrlL()
    {
        Assert.True(HotkeyParser.TryParse("Ctrl+L", out var hk));
        Assert.Equal(new ushort[] { 0x11 }, hk!.Modifiers.ToArray());
        Assert.Equal((ushort)'L', hk.Key);
    }

    [Fact]
    public void MultipleModifiers_CaseAndSpacesIgnored()
    {
        Assert.True(HotkeyParser.TryParse(" ctrl + SHIFT + i ", out var hk));
        Assert.Equal(new ushort[] { 0x11, 0x10 }, hk!.Modifiers.ToArray());
        Assert.Equal((ushort)'I', hk.Key);
    }

    [Fact]
    public void Digit()
    {
        Assert.True(HotkeyParser.TryParse("Alt+1", out var hk));
        Assert.Equal((ushort)'1', hk!.Key);
    }

    [Fact]
    public void FunctionKey()
    {
        Assert.True(HotkeyParser.TryParse("F6", out var hk));
        Assert.Empty(hk!.Modifiers.ToArray());
        Assert.Equal((ushort)Keys.F6, hk.Key);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Ctrl")]
    [InlineData("Ctrl+")]
    [InlineData("Hyper+L")]
    [InlineData("Ctrl+Bilinmeyen")]
    public void Invalid(string? text) => Assert.False(HotkeyParser.TryParse(text, out _));
}
