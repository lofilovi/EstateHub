using EstateHub_code.Data;
using EstateHub_code.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace EstateHubDesktop;

public partial class MainWindow : Window
{
    private const string ConnectionString = "Server=localhost;Database=EstateHub;User=root;Password=Toshatosha12!;";

    private readonly ObservableCollection<Apartment> _apartments = [];
    private readonly ObservableCollection<Tenant> _tenants = [];
    private readonly ObservableCollection<Property> _properties = [];

    private Tenant? _editingTenant;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await EnsureDatabaseColumnsAsync();
            await LoadAllAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Kunde inte starta EstateHub Desktop.\n\n{ex.Message}",
                "Databasfel",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString))
            .Options;

        return new AppDbContext(options);
    }

    private async Task LoadAllAsync()
    {
        await using var context = CreateContext();

        var properties = await context.Properties
            .OrderBy(property => property.Address)
            .ToListAsync();

        var tenants = await context.Tenants
            .OrderBy(tenant => tenant.FirstName)
            .ThenBy(tenant => tenant.LastName)
            .ToListAsync();

        var apartments = await context.Apartments
            .Include(apartment => apartment.Property)
            .Include(apartment => apartment.Tenant)
            .OrderBy(apartment => apartment.ApartmentNumber)
            .ToListAsync();

        ReplaceItems(_properties, properties);
        ReplaceItems(_tenants, tenants);
        ReplaceItems(_apartments, apartments);

        ApartmentPropertyComboBox.ItemsSource = _properties;
        ApartmentsGrid.ItemsSource = _apartments;
        TenantsGrid.ItemsSource = _tenants;

        UpdateDashboard();
    }

    private static void ReplaceItems<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();

        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void UpdateDashboard()
    {
        ApartmentCountText.Text = _apartments.Count.ToString();
        VacantCountText.Text = _apartments.Count(apartment => apartment.TenantID == null && apartment.Tenant == null).ToString();
        TenantCountText.Text = _tenants.Count.ToString();
        TotalRentText.Text = $"{_apartments.Sum(apartment => apartment.Rent):N0} kr";
    }

    private async Task EnsureDatabaseColumnsAsync()
    {
        await using var context = CreateContext();
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var apartmentColumns = new Dictionary<string, string>
        {
            ["Rooms"] = "int NOT NULL DEFAULT 0",
            ["Floor"] = "int NOT NULL DEFAULT 0",
            ["Status"] = "varchar(50) NOT NULL DEFAULT 'Ledig'",
            ["AvailableFrom"] = "datetime NULL",
            ["ElectricityIncluded"] = "tinyint(1) NOT NULL DEFAULT 0",
            ["WaterIncluded"] = "tinyint(1) NOT NULL DEFAULT 0",
            ["InternetIncluded"] = "tinyint(1) NOT NULL DEFAULT 0",
            ["Balcony"] = "tinyint(1) NOT NULL DEFAULT 0",
            ["Furnished"] = "tinyint(1) NOT NULL DEFAULT 0"
        };

        await EnsureColumnsAsync(connection, "apartments", apartmentColumns);

        var tenantColumns = new Dictionary<string, string>
        {
            ["PersonalNumber"] = "varchar(20) NULL",
            ["Address"] = "varchar(200) NULL",
            ["MobilePhone"] = "varchar(30) NULL"
        };

        await EnsureColumnsAsync(connection, "tenants", tenantColumns);
    }

    private static async Task EnsureColumnsAsync(System.Data.Common.DbConnection connection, string tableName, Dictionary<string, string> columns)
    {
        foreach (var column in columns)
        {
            if (await ColumnExistsAsync(connection, tableName, column.Key))
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            command.CommandText = $"ALTER TABLE `{tableName}` ADD COLUMN `{column.Key}` {column.Value};";
            await command.ExecuteNonQueryAsync();
        }
    }

    private static async Task<bool> ColumnExistsAsync(System.Data.Common.DbConnection connection, string tableName, string columnName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.columns
            WHERE table_schema = DATABASE()
                AND table_name = @tableName
                AND column_name = @columnName;
            """;

        var tableParameter = command.CreateParameter();
        tableParameter.ParameterName = "@tableName";
        tableParameter.Value = tableName;
        command.Parameters.Add(tableParameter);

        var columnParameter = command.CreateParameter();
        columnParameter.ParameterName = "@columnName";
        columnParameter.Value = columnName;
        command.Parameters.Add(columnParameter);

        return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
    }

    private void DashboardNavButton_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 0;
    }

    private void ApartmentsNavButton_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 1;
    }

    private void TenantsNavButton_Click(object sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 2;
    }

    private void GlobalSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApartmentSearchTextBox.Text = GlobalSearchTextBox.Text;
        TenantSearchTextBox.Text = GlobalSearchTextBox.Text;
        ApplyApartmentFilter();
        ApplyTenantFilter();
    }

    private void ToggleApartmentFormButton_Click(object sender, RoutedEventArgs e)
    {
        ApartmentFormPanel.Visibility = ApartmentFormPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private async void SaveApartmentButton_Click(object sender, RoutedEventArgs e)
    {
        if (ApartmentPropertyComboBox.SelectedValue is null)
        {
            MessageBox.Show("Välj en fastighet först.", "Saknar fastighet", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!double.TryParse(ApartmentSizeInput.Text, out var size) ||
            !decimal.TryParse(ApartmentRentInput.Text, out var rent) ||
            !int.TryParse(ApartmentRoomsInput.Text, out var rooms) ||
            !int.TryParse(ApartmentFloorInput.Text, out var floor))
        {
            MessageBox.Show("Kontrollera storlek, hyra, rum och våning.", "Fel format", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var apartment = new Apartment
        {
            ApartmentNumber = ApartmentNumberInput.Text.Trim(),
            PropertyID = Convert.ToInt32(ApartmentPropertyComboBox.SelectedValue),
            Size = size,
            Rent = rent,
            Rooms = rooms,
            Floor = floor,
            Status = SelectedComboText(ApartmentStatusInput) ?? "Ledig",
            AvailableFrom = ApartmentAvailableInput.SelectedDate,
            ElectricityIncluded = ElectricityIncludedInput.IsChecked == true,
            WaterIncluded = WaterIncludedInput.IsChecked == true,
            InternetIncluded = InternetIncludedInput.IsChecked == true,
            Balcony = BalconyInput.IsChecked == true,
            Furnished = FurnishedInput.IsChecked == true
        };

        await using var context = CreateContext();
        context.Apartments.Add(apartment);
        await context.SaveChangesAsync();

        ClearApartmentForm();
        ApartmentFormPanel.Visibility = Visibility.Collapsed;
        await LoadAllAsync();
    }

    private void ClearApartmentForm()
    {
        ApartmentNumberInput.Clear();
        ApartmentSizeInput.Clear();
        ApartmentRentInput.Clear();
        ApartmentRoomsInput.Text = "1";
        ApartmentFloorInput.Text = "0";
        ApartmentAvailableInput.SelectedDate = null;
        ElectricityIncludedInput.IsChecked = false;
        WaterIncludedInput.IsChecked = false;
        InternetIncludedInput.IsChecked = false;
        BalconyInput.IsChecked = false;
        FurnishedInput.IsChecked = false;
    }

    private void ApartmentSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyApartmentFilter();
    }

    private void ApartmentStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyApartmentFilter();
    }

    private void ApartmentSearchButton_Click(object sender, RoutedEventArgs e)
    {
        ApplyApartmentFilter();
    }

    private void ApplyApartmentFilter()
    {
        if (ApartmentsGrid is null)
        {
            return;
        }

        var query = ApartmentSearchTextBox?.Text?.Trim().ToLowerInvariant() ?? "";
        var status = SelectedComboText(ApartmentStatusFilter);
        var hasStatus = !string.IsNullOrWhiteSpace(status) && status != "Property Type...";

        ApartmentsGrid.ItemsSource = _apartments
            .Where(apartment =>
            {
                var text = string.Join(" ", apartment.ApartmentNumber, apartment.Property?.Address, apartment.Property?.City, apartment.Status, apartment.Tenant?.FirstName, apartment.Tenant?.LastName)
                    .ToLowerInvariant();

                return (!hasStatus || apartment.Status == status)
                    && (string.IsNullOrWhiteSpace(query) || text.Contains(query));
            })
            .ToList();
    }

    private void TenantSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyTenantFilter();
    }

    private void ClearTenantSearchButton_Click(object sender, RoutedEventArgs e)
    {
        TenantSearchTextBox.Clear();
    }

    private void ApplyTenantFilter()
    {
        if (TenantsGrid is null)
        {
            return;
        }

        var query = TenantSearchTextBox?.Text?.Trim().ToLowerInvariant() ?? "";
        TenantsGrid.ItemsSource = _tenants
            .Where(tenant =>
            {
                var text = string.Join(" ", tenant.FirstName, tenant.LastName, tenant.PersonalNumber, tenant.Address, tenant.MobilePhone, tenant.Phone, tenant.Email)
                    .ToLowerInvariant();

                return string.IsNullOrWhiteSpace(query) || text.Contains(query);
            })
            .ToList();
    }

    private async void SaveTenantButton_Click(object sender, RoutedEventArgs e)
    {
        await using var context = CreateContext();

        Tenant tenant;
        if (_editingTenant is null)
        {
            tenant = new Tenant();
            context.Tenants.Add(tenant);
        }
        else
        {
            tenant = await context.Tenants.FirstAsync(item => item.TenantID == _editingTenant.TenantID);
        }

        tenant.FirstName = TenantFirstNameInput.Text.Trim();
        tenant.LastName = TenantLastNameInput.Text.Trim();
        tenant.PersonalNumber = TenantPersonalNumberInput.Text.Trim();
        tenant.Address = TenantAddressInput.Text.Trim();
        tenant.MobilePhone = TenantMobileInput.Text.Trim();
        tenant.Phone = TenantPhoneInput.Text.Trim();
        tenant.Email = TenantEmailInput.Text.Trim();

        await context.SaveChangesAsync();

        ClearTenantForm();
        await LoadAllAsync();
    }

    private void EditTenantButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not Tenant tenant)
        {
            return;
        }

        _editingTenant = tenant;
        TenantFormTitle.Text = "Ändra hyresgäst";
        SaveTenantButton.Content = "Spara ändringar";
        TenantFirstNameInput.Text = tenant.FirstName ?? "";
        TenantLastNameInput.Text = tenant.LastName ?? "";
        TenantPersonalNumberInput.Text = tenant.PersonalNumber ?? "";
        TenantAddressInput.Text = tenant.Address ?? "";
        TenantMobileInput.Text = tenant.MobilePhone ?? "";
        TenantPhoneInput.Text = tenant.Phone ?? "";
        TenantEmailInput.Text = tenant.Email ?? "";
    }

    private async void DeleteTenantButton_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not Tenant tenant)
        {
            return;
        }

        var name = $"{tenant.FirstName} {tenant.LastName}".Trim();
        var result = MessageBox.Show(
            $"Vill du ta bort {name}? Lägenheter kopplade till hyresgästen blir lediga.",
            "Ta bort hyresgäst",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        await using var context = CreateContext();
        var dbTenant = await context.Tenants.FirstOrDefaultAsync(item => item.TenantID == tenant.TenantID);
        if (dbTenant is null)
        {
            return;
        }

        var apartments = await context.Apartments
            .Where(apartment => apartment.TenantID == tenant.TenantID)
            .ToListAsync();

        foreach (var apartment in apartments)
        {
            apartment.TenantID = null;
            apartment.Status = "Ledig";
        }

        context.Tenants.Remove(dbTenant);
        await context.SaveChangesAsync();

        if (_editingTenant?.TenantID == tenant.TenantID)
        {
            ClearTenantForm();
        }

        await LoadAllAsync();
    }

    private void CancelTenantButton_Click(object sender, RoutedEventArgs e)
    {
        ClearTenantForm();
    }

    private void ClearTenantForm()
    {
        _editingTenant = null;
        TenantFormTitle.Text = "Registrera ny hyresgäst";
        SaveTenantButton.Content = "Registrera";
        TenantFirstNameInput.Clear();
        TenantLastNameInput.Clear();
        TenantPersonalNumberInput.Clear();
        TenantAddressInput.Clear();
        TenantMobileInput.Clear();
        TenantPhoneInput.Clear();
        TenantEmailInput.Clear();
    }

    private static string? SelectedComboText(ComboBox comboBox)
    {
        return (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
    }
}
