using YTDownloader.Controller;

namespace YTDownloaderTest.ControllerTest;

public class AdjustableConcurrencyLimiterTest
{
    [Test]
    [Description("提高下載併發上限時，已等待中的下載應立即取得執行名額")]
    public async Task UpdateLimit_IncreaseLimit_ReleasesWaitingTask()
    {
        var limiter = new AdjustableConcurrencyLimiter(1);
        await limiter.WaitAsync();

        var waitingTask = limiter.WaitAsync();
        Assert.That(waitingTask.IsCompleted, Is.False);

        limiter.UpdateLimit(2);

        await waitingTask.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.That(waitingTask.IsCompletedSuccessfully, Is.True);

        limiter.Release();
        limiter.Release();
    }

    [Test]
    [Description("降低下載併發上限時，不會中斷既有下載，但會限制後續等待工作")]
    public async Task UpdateLimit_DecreaseLimit_BlocksNewTaskUntilActiveCountDropsBelowLimit()
    {
        var limiter = new AdjustableConcurrencyLimiter(2);
        await limiter.WaitAsync();
        await limiter.WaitAsync();

        limiter.UpdateLimit(1);

        var waitingTask = limiter.WaitAsync();
        Assert.That(waitingTask.IsCompleted, Is.False);

        limiter.Release();
        Assert.That(waitingTask.IsCompleted, Is.False);

        limiter.Release();
        await waitingTask.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.That(waitingTask.IsCompletedSuccessfully, Is.True);

        limiter.Release();
    }
}
