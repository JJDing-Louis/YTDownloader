using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.Extensions.Logging.Abstractions;
using YTDownloader;
using YTDownloader.Model;
using YTDownloader.Service;
using YTDownloader.UI;

namespace YTDownloaderTest.UITest;

public class ConfigFormTest
{
    private string? _configPath;

    [TearDown]
    public void TearDown()
    {
        if (!string.IsNullOrWhiteSpace(_configPath) && File.Exists(_configPath))
            File.Delete(_configPath);
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    [Description("ConfigForm 儲存目前設定時，應立即通知外層表單套用新的下載併發數量")]
    public void SaveCurrentSettings_RaisesSettingsAppliedWithUpdatedThreadSettings()
    {
        _configPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.json");
        var configService = new ConfigService(_configPath);
        using var form = new ConfigForm(configService);
        ConfigModel? appliedSettings = null;
        form.SettingsApplied += (_, settings) => appliedSettings = settings;

        GetPrivateField<NumericUpDown>(form, "_initialThreadNumeric").Value = 2;
        GetPrivateField<NumericUpDown>(form, "_maxThreadNumeric").Value = 5;

        var saved = InvokePrivate<bool>(form, "SaveCurrentSettings");

        Assert.That(saved, Is.True);
        Assert.That(appliedSettings, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(appliedSettings!.Thread.InitialCount, Is.EqualTo(2));
            Assert.That(appliedSettings.Thread.MaxCount, Is.EqualTo(5));
            Assert.That(configService.Load().Thread.MaxCount, Is.EqualTo(5));
        });
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        var field = target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"找不到欄位 {fieldName}");
        return (T)field!.GetValue(target)!;
    }

    private static T InvokePrivate<T>(object target, string methodName)
    {
        var method = target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, $"找不到方法 {methodName}");
        return (T)method!.Invoke(target, Array.Empty<object>())!;
    }
}
