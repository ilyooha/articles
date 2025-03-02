using Configuration.Services;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;

namespace Configuration.Tests;

[TestFixture]
public class ConfigurationProviderTests
{
    private readonly Mock<ISettingsProvider> _settingsProviderMock = new();

    [Test]
    public async Task TriggersChangeToken()
    {
        var settingsCallResults = new List<Dictionary<string, string?>>
        {
            // initial
            new() { { "k1", "v1-1" } },
            // not changed
            new() { { "k1", "v1-1" } },
            // changed
            new() { { "k1", "v1-2" } }
        };

        var expectedSettingsStream = new List<Dictionary<string, string?>>
        {
            new() { { "k1", "v1-1" } },
            new() { { "k1", "v1-2" } }
        };

        var sequenceSetup = _settingsProviderMock.SetupSequence(x => x.GetSettings());
        foreach (var state in settingsCallResults)
            sequenceSetup.Returns(state);

        var actualSettingsStream = new List<Dictionary<string, string?>>();

        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
        using (var provider =
               new AutoReloadingConfigurationProvider(_settingsProviderMock.Object, TimeSpan.FromMilliseconds(100)))
        using (ChangeToken.OnChange(
                   provider.GetReloadToken,
                   () =>
                   {
                       var state = new Dictionary<string, string?>();
                       foreach (var key in provider.GetChildKeys([], null))
                       {
                           if (provider.TryGet(key, out var value))
                               state[key] = value;
                       }

                       actualSettingsStream.Add(state);
                   }))
        {   
            while (actualSettingsStream.Count < expectedSettingsStream.Count && !cts.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromMilliseconds(10), cts.Token);
        }

        actualSettingsStream.Should()
            .BeEquivalentTo(expectedSettingsStream);
    }
}