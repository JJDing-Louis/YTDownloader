namespace YTDownloader.UI.CustomUI;

/// <summary>
///     在 DataGridView 儲存格內繪製進度條的自訂儲存格。
///     <para>
///         Value 應為 <see cref="double" />，範圍 0–100。
///     </para>
/// </summary>
public class DataGridViewProgressBarCell : DataGridViewTextBoxCell
{
    public override object DefaultNewRowValue => 0.0;

    protected override void Paint(
        Graphics graphics,
        Rectangle clipBounds,
        Rectangle cellBounds,
        int rowIndex,
        DataGridViewElementStates cellState,
        object? value,
        object? formattedValue,
        string? errorText,
        DataGridViewCellStyle cellStyle,
        DataGridViewAdvancedBorderStyle advancedBorderStyle,
        DataGridViewPaintParts paintParts)
    {
        // 只保留邊框由 base 繪製，其餘全部自行控制
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState,
            value, formattedValue, errorText, cellStyle,
            advancedBorderStyle, DataGridViewPaintParts.Border);

        // ── 取得百分比值 ──────────────────────────────────────
        var pct = value switch
        {
            double d => d,
            float f => f,
            int i => i,
            _ => 0.0
        };
        pct = Math.Clamp(pct, 0.0, 100.0);

        // ── 計算內部繪圖區域 ──────────────────────────────────
        const int pad = 3;
        var inner = new Rectangle(
            cellBounds.X + pad,
            cellBounds.Y + pad,
            cellBounds.Width - pad * 2,
            cellBounds.Height - pad * 2);

        if (inner.Width <= 0 || inner.Height <= 0)
            return;

        // ── 灰底 ──────────────────────────────────────────────
        graphics.FillRectangle(Brushes.WhiteSmoke, inner);

        // ── 進度填色 ──────────────────────────────────────────
        if (pct > 0)
        {
            var barW = Math.Max(1, (int)(inner.Width * pct / 100.0));

            // 主色：藍色進度條
            using var fillBrush = new SolidBrush(Color.FromArgb(99, 162, 245));
            graphics.FillRectangle(fillBrush, inner.X, inner.Y, barW, inner.Height);

            // 上半部亮面（模擬 3D 效果）
            using var highlightBrush = new SolidBrush(Color.FromArgb(70, Color.White));
            graphics.FillRectangle(highlightBrush,
                inner.X, inner.Y, barW, Math.Max(1, inner.Height / 2));
        }

        // ── 外框 ──────────────────────────────────────────────
        using var borderPen = new Pen(Color.FromArgb(180, 180, 180));
        graphics.DrawRectangle(borderPen,
            inner.X, inner.Y, inner.Width - 1, inner.Height - 1);

        // ── 百分比文字 ────────────────────────────────────────
        var fmt = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        // 進度超過半條時用白字，否則深灰
        using var textBrush = new SolidBrush(
            pct > 55 ? Color.White : Color.FromArgb(50, 50, 50));

        graphics.DrawString(
            $"{pct:F0}%",
            cellStyle.Font ?? SystemFonts.DefaultFont,
            textBrush,
            cellBounds,
            fmt);
    }
}

/// <summary>
///     承載 <see cref="DataGridViewProgressBarCell" /> 的欄位類型。
/// </summary>
public class DataGridViewProgressBarColumn : DataGridViewTextBoxColumn
{
    public DataGridViewProgressBarColumn()
    {
        CellTemplate = new DataGridViewProgressBarCell();
        ReadOnly = true;
    }
}