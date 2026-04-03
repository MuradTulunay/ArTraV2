using ArTraV2.Core.Indicators;
using ArTraV2.Core.Indicators.Impl;

namespace ArTraV2.App.Dialogs;

public class IndicatorSelectorDialog : Form
{
    private readonly ListBox _lstAvailable = new();
    private readonly ListBox _lstActive = new();
    private readonly Button _btnAdd = new();
    private readonly Button _btnRemove = new();
    private readonly Button _btnOk = new();

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public List<IIndicator> ActiveIndicators { get; set; } = [];

    private static readonly (string Name, Func<IIndicator> Factory)[] AvailableIndicators =
    [
        ("SMA - Simple Moving Average", () => new SmaIndicator()),
        ("EMA - Exponential Moving Average", () => new EmaIndicator()),
        ("RSI - Relative Strength Index", () => new RsiIndicator()),
        ("MACD", () => new MacdIndicator()),
        ("Bollinger Bands", () => new BollingerBandsIndicator()),
    ];

    public IndicatorSelectorDialog()
    {
        Text = "Indicators";
        Size = new Size(500, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 34, 45);
        ForeColor = Color.FromArgb(209, 212, 220);
        MaximizeBox = false;
        MinimizeBox = false;

        var lblAvail = new Label { Text = "Available", Location = new Point(12, 10), AutoSize = true };
        var lblActive = new Label { Text = "Active", Location = new Point(270, 10), AutoSize = true };

        _lstAvailable.Location = new Point(12, 30);
        _lstAvailable.Size = new Size(200, 250);
        _lstAvailable.BackColor = Color.FromArgb(42, 46, 57);
        _lstAvailable.ForeColor = Color.White;
        _lstAvailable.BorderStyle = BorderStyle.FixedSingle;
        foreach (var (name, _) in AvailableIndicators)
            _lstAvailable.Items.Add(name);

        _lstActive.Location = new Point(270, 30);
        _lstActive.Size = new Size(200, 250);
        _lstActive.BackColor = Color.FromArgb(42, 46, 57);
        _lstActive.ForeColor = Color.White;
        _lstActive.BorderStyle = BorderStyle.FixedSingle;

        _btnAdd.Text = "Add >";
        _btnAdd.Location = new Point(218, 100);
        _btnAdd.Size = new Size(46, 28);
        _btnAdd.FlatStyle = FlatStyle.Flat;
        _btnAdd.BackColor = Color.FromArgb(38, 166, 91);
        _btnAdd.ForeColor = Color.White;
        _btnAdd.Click += BtnAdd_Click;

        _btnRemove.Text = "< Del";
        _btnRemove.Location = new Point(218, 135);
        _btnRemove.Size = new Size(46, 28);
        _btnRemove.FlatStyle = FlatStyle.Flat;
        _btnRemove.BackColor = Color.FromArgb(231, 76, 60);
        _btnRemove.ForeColor = Color.White;
        _btnRemove.Click += BtnRemove_Click;

        _btnOk.Text = "OK";
        _btnOk.Location = new Point(390, 300);
        _btnOk.Size = new Size(80, 30);
        _btnOk.FlatStyle = FlatStyle.Flat;
        _btnOk.BackColor = Color.FromArgb(41, 98, 255);
        _btnOk.ForeColor = Color.White;
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Click += (s, e) => Close();

        Controls.AddRange([lblAvail, lblActive, _lstAvailable, _lstActive, _btnAdd, _btnRemove, _btnOk]);
        AcceptButton = _btnOk;
    }

    public void LoadActiveIndicators(List<IIndicator> indicators)
    {
        ActiveIndicators = new List<IIndicator>(indicators);
        RefreshActiveList();
    }

    private void RefreshActiveList()
    {
        _lstActive.Items.Clear();
        foreach (var ind in ActiveIndicators)
            _lstActive.Items.Add(ind.ShortName);
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        if (_lstAvailable.SelectedIndex < 0) return;
        var (_, factory) = AvailableIndicators[_lstAvailable.SelectedIndex];
        ActiveIndicators.Add(factory());
        RefreshActiveList();
    }

    private void BtnRemove_Click(object? sender, EventArgs e)
    {
        if (_lstActive.SelectedIndex < 0 || _lstActive.SelectedIndex >= ActiveIndicators.Count) return;
        ActiveIndicators.RemoveAt(_lstActive.SelectedIndex);
        RefreshActiveList();
    }
}
