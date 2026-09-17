using Microsoft.Maui.Devices;
#if WINDOWS
using System.Management;
#endif

namespace FreeFireOptimizer;

public partial class MainPage : ContentPage
{
    private string lastSettings = "";

    public MainPage() => InitializeComponent();

    private void OnXChanged(object sender, ValueChangedEventArgs e) => XValue.Text = Math.Round(e.NewValue).ToString();
    private void OnYChanged(object sender, ValueChangedEventArgs e) => YValue.Text = Math.Round(e.NewValue).ToString();

    private async void OnDetectClicked(object sender, EventArgs e)
    {
        DeviceStatus.Text = "Cihaz bilgileri okunuyor...";
        string model = DeviceInfo.Current.Model;
        string manufacturer = DeviceInfo.Current.Manufacturer;
        string platform = DeviceInfo.Current.Platform.ToString();
        string version = DeviceInfo.Current.VersionString;

#if WINDOWS
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            foreach (ManagementObject item in searcher.Get())
            {
                if (string.IsNullOrWhiteSpace(CpuEntry.Text))
                    CpuEntry.Text = item["Name"]?.ToString() ?? "";
                break;
            }
        }
        catch { }
#endif

        DeviceStatus.Text = $"{manufacturer} {model} • {platform} {version}";
        await DisplayAlert("Cihaz bilgileri", "Desteklenen bilgiler alındı. Eksik CPU veya GPU bilgilerini manuel girebilirsin.", "Tamam");
    }

    private void OnCalculateClicked(object sender, EventArgs e)
    {
        int ram = GetRam();
        int dpi = int.Parse(DpiPicker.SelectedItem?.ToString() ?? "800");
        int logicalCores = 4;

#if WINDOWS
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT NumberOfLogicalProcessors FROM Win32_Processor");
            foreach (ManagementObject item in searcher.Get())
            {
                logicalCores = Convert.ToInt32(item["NumberOfLogicalProcessors"]);
                break;
            }
        }
        catch { }
#endif

        int cpuCores = Math.Max(2, Math.Min(8, logicalCores / 2));
        int allocatedRam = Math.Max(2, Math.Min(ram / 2, ram - 1));
        int currentX = (int)XSlider.Value;
        int currentY = (int)YSlider.Value;

        int general = Math.Clamp(96 - (dpi - 800) / 150, 75, 100);
        int redDot = Math.Clamp(88 - (dpi - 800) / 200, 65, 95);
        int scope2x = Math.Clamp(78 - (dpi - 800) / 250, 55, 90);
        int scope4x = Math.Clamp(65 - (dpi - 800) / 350, 45, 85);
        int awm = Math.Clamp(52 - (dpi - 800) / 400, 35, 75);
        int emulatorX = Math.Clamp((int)(currentX * .45 + 55 + (800 - dpi) * .012), 30, 100);
        int emulatorY = Math.Clamp((int)(currentY * .4 + 48 + (800 - dpi) * .009), 25, 100);

        string renderer = ram <= 4 ? "OpenGL" : "Auto / OpenGL";

        EngineResult.Text = $"CPU çekirdek tahsisi: {cpuCores}\nRAM tahsisi: {allocatedRam} GB\nGrafik modu: {renderer}\nÇözünürlük: 1920 × 1080\nDPI: 240\nProfil: {(ram <= 4 ? "Düşük bellek" : "Dengeli performans")}";
        SensitivityResult.Text = $"Genel: {general}\nKırmızı Nokta: {redDot}\n2x Scope: {scope2x}\n4x Scope: {scope4x}\nAWM Scope: {awm}\nEmülatör X: {emulatorX}\nEmülatör Y: {emulatorY}";

        lastSettings = $"FREE FIRE OPTIMIZER\n\nCPU: {CpuEntry.Text}\nGPU: {GpuEntry.Text}\nRAM: {ram} GB\nEmülatör: {EmulatorPicker.SelectedItem}\nMouse DPI: {dpi}\n\nCPU cores: {cpuCores}\nRAM allocation: {allocatedRam} GB\nGraphics: {renderer}\n\nGeneral: {general}\nRed Dot: {redDot}\n2x Scope: {scope2x}\n4x Scope: {scope4x}\nAWM: {awm}\nEmulator X: {emulatorX}\nEmulator Y: {emulatorY}\n\nKeymapping reference: 16458, 2, 10888";

        ResultsCard.IsVisible = true;
        ActionStatus.Text = "Ayarlar hesaplandı. Eğitim alanında test ederek düzenleyebilirsin.";
    }

    private int GetRam()
    {
        string value = RamPicker.SelectedItem?.ToString() ?? "8 GB";
        if (value.StartsWith("4")) return 4;
        if (value.StartsWith("16")) return 16;
        if (value.StartsWith("32")) return 32;
        return 8;
    }

    private async void OnCopyClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(lastSettings)) { ActionStatus.Text = "Önce ayarları hesapla."; return; }
        await Clipboard.Default.SetTextAsync(lastSettings);
        ActionStatus.Text = "Ayarlar panoya kopyalandı.";
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(lastSettings)) { ActionStatus.Text = "Önce ayarları hesapla."; return; }
        Preferences.Default.Set("last_settings", lastSettings);
        ActionStatus.Text = "Ayarlar bu cihazda kaydedildi.";
    }
}
