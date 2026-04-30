using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using YTDownloader;

namespace YTDownloaderTest.UITest;

public class MainFormTest
{
    [Test]
    [Apartment(ApartmentState.STA)]
    [Description("清空已完成應只移除狀態為已完成的列，且不可因欄位名稱或集合修改而拋出例外")]
    public void ClearCompleteTask_RemovesOnlyCompletedRows()
    {
        using var grid = CreateDownloadListGrid();
        grid.Rows.Add(1, "完成項目", "已完成");
        grid.Rows.Add(2, "下載中項目", "下載中");
        grid.Rows.Add(3, "英文完成項目", "Complete");

        var mainForm = (MainForm)RuntimeHelpers.GetUninitializedObject(typeof(MainForm));
        SetPrivateField(mainForm, "dGV_DownloadList", grid);

        Assert.DoesNotThrow(() => InvokePrivate(mainForm, "btn_ClearCompleteTask_Click", mainForm, EventArgs.Empty));
        Assert.Multiple(() =>
        {
            Assert.That(grid.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow).Count(), Is.EqualTo(1));
            Assert.That(grid.Rows[0].Cells["colTitle"].Value, Is.EqualTo("下載中項目"));
            Assert.That(grid.Rows[0].Cells["colIndex"].Value, Is.EqualTo(1));
        });
    }

    private static DataGridView CreateDownloadListGrid()
    {
        var grid = new DataGridView { AllowUserToAddRows = false };
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colIndex" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus" });
        return grid;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"找不到欄位 {fieldName}");
        field!.SetValue(target, value);
    }

    private static void InvokePrivate(object target, string methodName, params object[] args)
    {
        var method = target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, $"找不到方法 {methodName}");
        method!.Invoke(target, args);
    }
}
