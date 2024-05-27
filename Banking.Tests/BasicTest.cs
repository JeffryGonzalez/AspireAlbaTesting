using Alba;

namespace Banking.Tests;
public class BasicTest : IClassFixture<AlbaFixture>
{

    private IAlbaHost Host;
    public BasicTest(AlbaFixture fixture)
    {
        Host = fixture.Host;
    }

  

    [Fact]
    public async Task GettingUser()
    {
        await Host.Scenario(api =>
        {
            api.Get.Url("/users");
        });
    }
}
