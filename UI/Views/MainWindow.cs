using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using practiceStudioAssetManager.Core.Constants;
using practiceStudioAssetManager.Core.Entities;
using practiceStudioAssetManager.Infrastructure.Data;

namespace practiceStudioAssetManager.UI.Views;

public class MainWindow : Window
{
    private readonly AppDbContext _db = new();
    
    private readonly DataGrid _hardwareGrid = new() { AutoGenerateColumns = true, IsReadOnly = false, CanUserResizeColumns = true };
    private readonly DataGrid _softwareGrid = new() { AutoGenerateColumns = true, IsReadOnly = false, CanUserResizeColumns = true };
    private readonly DataGrid _engineerGrid = new() { AutoGenerateColumns = true, IsReadOnly = false, CanUserResizeColumns = true };
    private readonly DataGrid _clientGrid = new() { AutoGenerateColumns = true, IsReadOnly = false, CanUserResizeColumns = true };
    
    private readonly DataGrid _sessionGrid = new() { AutoGenerateColumns = true, IsReadOnly = true, CanUserResizeColumns = true };

    private readonly TextBox _searchBox = new() { Watermark = "Global Database Search...", Margin = new Avalonia.Thickness(0,0,0,10) };

    private readonly ComboBox _sessionClientCombo = new() { HorizontalAlignment = HorizontalAlignment.Stretch, PlaceholderText = "Select Client" };
    private readonly ListBox _sessionHwList = new() { SelectionMode = SelectionMode.Multiple, Height = 100 };
    private readonly ListBox _sessionEngList = new() { SelectionMode = SelectionMode.Multiple, Height = 100 };

    public MainWindow()
    {
        Title = AppConstants.AppTitle;
        Width = 1200;
        Height = 800;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _searchBox.KeyUp += (s, e) => RefreshAllGrids(_searchBox.Text ?? "");

        AttachInlineEditing(_clientGrid, ClientGrid_CellEditEnded);
        AttachInlineEditing(_hardwareGrid, HardwareGrid_CellEditEnded);
        AttachInlineEditing(_softwareGrid, SoftwareGrid_CellEditEnded);
        AttachInlineEditing(_engineerGrid, EngineerGrid_CellEditEnded);

        var topPanel = new StackPanel { Margin = new Avalonia.Thickness(UIMetrics.DefaultPadding) };
        topPanel.Children.Add(new TextBlock { Text = "Global Search", FontWeight = Avalonia.Media.FontWeight.Bold });
        topPanel.Children.Add(_searchBox);

        var tabControl = new TabControl();
        tabControl.ItemsSource = new List<TabItem>
        {
            new() { Header = "Clients", Content = CreateClientsTab() },
            new() { Header = "Hardware", Content = CreateHardwareTab() },
            new() { Header = "Software", Content = CreateSoftwareTab() },
            new() { Header = "Engineers", Content = CreateEngineersTab() },
            new() { Header = "Studio Sessions", Content = CreateSessionsTab() }
        };
        
        var rootLayout = new DockPanel();
        DockPanel.SetDock(topPanel, Dock.Top);
        rootLayout.Children.Add(topPanel);
        rootLayout.Children.Add(tabControl);

        Content = rootLayout;
        RefreshAllGrids();
    }
    private void AttachInlineEditing(DataGrid grid, EventHandler<DataGridCellEditEndedEventArgs> editHandler)
    {
        grid.AutoGeneratingColumn += (s, e) => { if (e.Column.Header?.ToString() == "Id") e.Column.IsReadOnly = true; };
        grid.CellEditEnded += editHandler;
    }

    private void ClientGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext is ClientDto dto && _db.Clients.Find(dto.Id) is Client entity) {
            entity.FullName = dto.FullName ?? ""; entity.Phone = dto.Phone ?? ""; entity.Email = dto.Email ?? "";
            _db.SaveChanges();
        }
    }

    private void HardwareGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext is HardwareDto dto && _db.Equipments.Find(dto.Id) is Hardware entity) {
            entity.Name = dto.Name ?? ""; entity.SerialNumber = dto.SerialNumber ?? ""; entity.Location = dto.Location ?? "";
            if (Enum.TryParse(dto.Category, out EquipmentCategory cat)) entity.Category = cat;
            if (Enum.TryParse(dto.Status, out EquipmentStatus stat)) entity.Status = stat;
            _db.SaveChanges();
        }
    }

    private void SoftwareGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext is SoftwareDto dto && _db.Licenses.Find(dto.Id) is Software entity) {
            entity.Title = dto.Title ?? ""; entity.Developer = dto.Developer ?? ""; entity.AuthMethod = dto.AuthMethod ?? ""; entity.Workstation = dto.Workstation ?? "";
            if (Enum.TryParse(dto.LicenseType, out LicenseType type)) entity.LicenseType = type;
            _db.SaveChanges();
        }
    }

    private void EngineerGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext is EngineerDto dto && _db.Engineers.Find(dto.Id) is Engineer entity) {
            entity.FullName = dto.FullName ?? ""; entity.Role = dto.Role ?? ""; entity.Phone = dto.Phone ?? ""; entity.Email = dto.Email ?? "";
            _db.SaveChanges();
        }
    }

    private Control CreateGridContainer(Control topForm, DataGrid dataGrid, Action deleteAction)
    {
        var grid = new Grid { Margin = new Avalonia.Thickness(UIMetrics.DefaultPadding) };
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        Grid.SetRow(topForm, 0); Grid.SetRow(dataGrid, 1);
        
        var deleteBtn = new Button { Content = "Delete Selected", HorizontalAlignment = HorizontalAlignment.Right, Background = Avalonia.Media.Brushes.DarkRed, Margin = new Avalonia.Thickness(0,8,0,0) };
        deleteBtn.Click += (s, e) => deleteAction();
        Grid.SetRow(deleteBtn, 2);

        grid.Children.Add(topForm); grid.Children.Add(dataGrid); grid.Children.Add(deleteBtn);
        return grid;
    }

    private Control CreateClientsTab()
    {
        var form = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0,0,0,16) };
        var nameInput = new TextBox { Watermark = "Client Full Name" };
        var phoneInput = new TextBox { Watermark = "Phone" };
        var emailInput = new TextBox { Watermark = "Email" };
        var addBtn = new Button { Content = "Register Client", HorizontalAlignment = HorizontalAlignment.Stretch };

        addBtn.Click += (s, e) => {
            if (string.IsNullOrWhiteSpace(nameInput.Text)) return;
            _db.Clients.Add(new Client { FullName = nameInput.Text, Phone = phoneInput.Text ?? "", Email = emailInput.Text ?? "" });
            _db.SaveChanges(); RefreshAllGrids();
            nameInput.Text = phoneInput.Text = emailInput.Text = string.Empty;
        };

        form.Children.Add(nameInput); form.Children.Add(phoneInput); form.Children.Add(emailInput); form.Children.Add(addBtn);
        
        return CreateGridContainer(form, _clientGrid, () => {
            if (_clientGrid.SelectedItem is ClientDto c && _db.Clients.Find(c.Id) is Client entity) {
                _db.Clients.Remove(entity); _db.SaveChanges(); RefreshAllGrids(); 
            }
        });
    }

    private Control CreateHardwareTab()
    {
        var form = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0,0,0,16) };
        var nameInput = new TextBox { Watermark = "Hardware Name" };
        var serialInput = new TextBox { Watermark = "Serial Number" };
        var locationInput = new TextBox { Watermark = "Storage Location" };
        var categoryCombo = new ComboBox { ItemsSource = Enum.GetValues(typeof(EquipmentCategory)), SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Stretch };
        var addBtn = new Button { Content = "Register Hardware", HorizontalAlignment = HorizontalAlignment.Stretch };

        addBtn.Click += (s, e) => {
            if (string.IsNullOrWhiteSpace(nameInput.Text)) return;
            _db.Equipments.Add(new Hardware { Name = nameInput.Text, SerialNumber = serialInput.Text ?? "", Location = locationInput.Text ?? "Main Storage", Category = (EquipmentCategory)categoryCombo.SelectedItem! });
            _db.SaveChanges(); RefreshAllGrids();
            nameInput.Text = serialInput.Text = locationInput.Text = string.Empty;
        };

        form.Children.Add(nameInput); form.Children.Add(categoryCombo); form.Children.Add(serialInput); form.Children.Add(locationInput); form.Children.Add(addBtn);
        
        return CreateGridContainer(form, _hardwareGrid, () => {
            if (_hardwareGrid.SelectedItem is HardwareDto h && _db.Equipments.Find(h.Id) is Hardware entity) {
                _db.Equipments.Remove(entity); _db.SaveChanges(); RefreshAllGrids(); 
            }
        });
    }

    private Control CreateSoftwareTab()
    {
        var form = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0,0,0,16) };
        var titleInput = new TextBox { Watermark = "Title" };
        var devInput = new TextBox { Watermark = "Developer" };
        var authInput = new TextBox { Watermark = "Auth Method" };
        var stationInput = new TextBox { Watermark = "Workstation Location" };
        var typeCombo = new ComboBox { ItemsSource = Enum.GetValues(typeof(LicenseType)), SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Stretch };
        var addBtn = new Button { Content = "Register License", HorizontalAlignment = HorizontalAlignment.Stretch };

        addBtn.Click += (s, e) => {
            if (string.IsNullOrWhiteSpace(titleInput.Text)) return;
            _db.Licenses.Add(new Software { Title = titleInput.Text, Developer = devInput.Text ?? "", AuthMethod = authInput.Text ?? "Digital", Workstation = stationInput.Text ?? "Studio PC A", LicenseType = (LicenseType)typeCombo.SelectedItem! });
            _db.SaveChanges(); RefreshAllGrids();
            titleInput.Text = devInput.Text = authInput.Text = stationInput.Text = string.Empty;
        };

        form.Children.Add(titleInput); form.Children.Add(devInput); form.Children.Add(typeCombo); form.Children.Add(authInput); form.Children.Add(stationInput); form.Children.Add(addBtn);
        
        return CreateGridContainer(form, _softwareGrid, () => {
            if (_softwareGrid.SelectedItem is SoftwareDto sw && _db.Licenses.Find(sw.Id) is Software entity) {
                _db.Licenses.Remove(entity); _db.SaveChanges(); RefreshAllGrids(); 
            }
        });
    }

    private Control CreateEngineersTab()
    {
        var form = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0,0,0,16) };
        var nameInput = new TextBox { Watermark = "Full Name" };
        var roleInput = new TextBox { Watermark = "Role" };
        var phoneInput = new TextBox { Watermark = "Phone" };
        var emailInput = new TextBox { Watermark = "Email" };
        var addBtn = new Button { Content = "Add Staff", HorizontalAlignment = HorizontalAlignment.Stretch };

        addBtn.Click += (s, e) => {
            if (string.IsNullOrWhiteSpace(nameInput.Text)) return;
            _db.Engineers.Add(new Engineer { FullName = nameInput.Text, Role = roleInput.Text ?? "", Phone = phoneInput.Text ?? "", Email = emailInput.Text ?? "" });
            _db.SaveChanges(); RefreshAllGrids();
            nameInput.Text = roleInput.Text = phoneInput.Text = emailInput.Text = string.Empty;
        };

        form.Children.Add(nameInput); form.Children.Add(roleInput); form.Children.Add(phoneInput); form.Children.Add(emailInput); form.Children.Add(addBtn);
        
        return CreateGridContainer(form, _engineerGrid, () => {
            if (_engineerGrid.SelectedItem is EngineerDto en && _db.Engineers.Find(en.Id) is Engineer entity) {
                _db.Engineers.Remove(entity); _db.SaveChanges(); RefreshAllGrids(); 
            }
        });
    }

    private Control CreateSessionsTab()
    {
        var grid = new Grid { Margin = new Avalonia.Thickness(UIMetrics.DefaultPadding) };
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        var form = new StackPanel { Spacing = 8, Margin = new Avalonia.Thickness(0,0,0,16) };
        var projectInput = new TextBox { Watermark = "Project / Track Name" };
        
        var startBtn = new Button { Content = "Start Studio Session", HorizontalAlignment = HorizontalAlignment.Stretch };
        startBtn.Click += (s, e) => {
            if (_sessionClientCombo.SelectedItem is Client client)
            {
                var selectedHw = _sessionHwList.SelectedItems?.Cast<Hardware>().ToList() ?? new List<Hardware>();
                var selectedEng = _sessionEngList.SelectedItems?.Cast<Engineer>().ToList() ?? new List<Engineer>();

                foreach (var hw in selectedHw) hw.Status = EquipmentStatus.InUse;

                var session = new StudioSession { ClientId = client.Id, ProjectName = projectInput.Text ?? "" };
                session.Equipments.AddRange(selectedHw);
                session.Engineers.AddRange(selectedEng);
                
                _db.StudioSessions.Add(session);
                _db.SaveChanges(); RefreshAllGrids();
                _sessionClientCombo.SelectedItem = null; projectInput.Text = string.Empty;
                _sessionHwList.SelectedItems?.Clear(); _sessionEngList.SelectedItems?.Clear();
            }
        };

        form.Children.Add(new TextBlock{Text="Project Details:"}); form.Children.Add(projectInput);
        form.Children.Add(new TextBlock{Text="Client:"}); form.Children.Add(_sessionClientCombo);
        form.Children.Add(new TextBlock{Text="Select Hardware (Ctrl+Click for multiple):"}); form.Children.Add(_sessionHwList);
        form.Children.Add(new TextBlock{Text="Select Engineers (Ctrl+Click for multiple):"}); form.Children.Add(_sessionEngList);
        form.Children.Add(startBtn);

        var actionPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8, Margin = new Avalonia.Thickness(0,8,0,0) };
        var endBtn = new Button { Content = "End Selected Session (Return All)", Background = Avalonia.Media.Brushes.DarkOrange };
        var deleteBtn = new Button { Content = "Delete Record", Background = Avalonia.Media.Brushes.DarkRed };

        endBtn.Click += (s, e) => {
            if (_sessionGrid.SelectedItem is SessionViewDto row && _db.StudioSessions.Include(x => x.Equipments).FirstOrDefault(x => x.Id == row.Id) is StudioSession session)
            {
                if (session.IsActive) {
                    session.ReturnedAt = DateTime.UtcNow;
                    foreach (var hw in session.Equipments) hw.Status = EquipmentStatus.Available;
                    _db.SaveChanges(); RefreshAllGrids();
                }
            }
        };

        deleteBtn.Click += (s, e) => {
            if (_sessionGrid.SelectedItem is SessionViewDto row && _db.StudioSessions.Include(x => x.Equipments).FirstOrDefault(x => x.Id == row.Id) is StudioSession session)
            {
                if (session.IsActive) foreach (var hw in session.Equipments) hw.Status = EquipmentStatus.Available;
                _db.StudioSessions.Remove(session); _db.SaveChanges(); RefreshAllGrids();
            }
        };

        actionPanel.Children.Add(endBtn); actionPanel.Children.Add(deleteBtn);

        Grid.SetRow(form, 0); Grid.SetRow(_sessionGrid, 1); Grid.SetRow(actionPanel, 2);
        grid.Children.Add(form); grid.Children.Add(_sessionGrid); grid.Children.Add(actionPanel);
        return grid;
    }

    private void RefreshAllGrids(string filter = "")
    {
        var s = filter.ToLower();

        try 
        {
            _clientGrid.ItemsSource = _db.Clients.ToList()
                .Where(c => string.IsNullOrEmpty(s) || (c.FullName?.ToLower().Contains(s) == true) || (c.Email?.ToLower().Contains(s) == true) || (c.Phone?.Contains(s) == true))
                .Select(c => new ClientDto { Id = c.Id, FullName = c.FullName ?? "", Email = c.Email ?? "", Phone = c.Phone ?? "" }).ToList();
                
            _hardwareGrid.ItemsSource = _db.Equipments.ToList()
                .Where(h => string.IsNullOrEmpty(s) || (h.Name?.ToLower().Contains(s) == true) || (h.SerialNumber?.ToLower().Contains(s) == true) || (h.Location?.ToLower().Contains(s) == true) || h.Category.ToString().ToLower().Contains(s))
                .Select(h => new HardwareDto { Id = h.Id, Name = h.Name ?? "", Category = h.Category.ToString(), SerialNumber = h.SerialNumber ?? "", Location = h.Location ?? "", Status = h.Status.ToString() }).ToList();
                
            _softwareGrid.ItemsSource = _db.Licenses.ToList()
                .Where(l => string.IsNullOrEmpty(s) || (l.Title?.ToLower().Contains(s) == true) || (l.Developer?.ToLower().Contains(s) == true) || (l.Workstation?.ToLower().Contains(s) == true) || (l.AuthMethod?.ToLower().Contains(s) == true))
                .Select(l => new SoftwareDto { Id = l.Id, Title = l.Title ?? "", Developer = l.Developer ?? "", LicenseType = l.LicenseType.ToString(), AuthMethod = l.AuthMethod ?? "", Workstation = l.Workstation ?? "" }).ToList();
                
            _engineerGrid.ItemsSource = _db.Engineers.ToList()
                .Where(e => string.IsNullOrEmpty(s) || (e.FullName?.ToLower().Contains(s) == true) || (e.Role?.ToLower().Contains(s) == true) || (e.Email?.ToLower().Contains(s) == true) || (e.Phone?.Contains(s) == true))
                .Select(e => new EngineerDto { Id = e.Id, FullName = e.FullName ?? "", Role = e.Role ?? "", Phone = e.Phone ?? "", Email = e.Email ?? "" }).ToList();

            _sessionGrid.ItemsSource = _db.StudioSessions.Include(x => x.Client).Include(x => x.Equipments).Include(x => x.Engineers).ToList()
                .Where(x => string.IsNullOrEmpty(s) || (x.Client?.FullName?.ToLower().Contains(s) == true) || (x.ProjectName?.ToLower().Contains(s) == true))
                .Select(x => new SessionViewDto { Id = x.Id, Status = x.IsActive ? "ACTIVE" : "ENDED", Project = x.ProjectName ?? "", Client = x.Client?.FullName ?? "Unknown", HardwareCount = x.Equipments.Count, EngineersCount = x.Engineers.Count, Start = x.CheckedOutAt.ToString("g"), End = x.ReturnedAt?.ToString("g") ?? "-" }).ToList();

            _sessionClientCombo.ItemsSource = _db.Clients.ToList();
            _sessionHwList.ItemsSource = _db.Equipments.Where(h => h.Status == EquipmentStatus.Available).ToList();
            _sessionEngList.ItemsSource = _db.Engineers.ToList();
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Search error: {ex.Message}");
        }
    }
}