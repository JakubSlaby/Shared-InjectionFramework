namespace WhiteSparrow.Shared.DependencyInjection.Tests.Mock
{
    public class TestPlayer
    {
        [InjectApplication] 
        public ITestPlayerModel PlayerModel;

        [InjectApplication] 
        public ITestCurrencyModel Currency;

        [InjectNetworking] 
        public ITestMultiplayerManager MultiplayerManager;
    }
}