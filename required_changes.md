# Required changes: user-selectable TabT/AFTT season for database updates

This document describes the changes needed so that the user can pick which
competition season to use when updating the database from the internet
(VTTL/AFTT), and so that this selection is honored consistently everywhere
the app talks to `TabTConnector`. No code has been changed — this is a
plan to implement by hand (or to feed back into an assistant) together with
concrete code snippets.

## 0. Summary of the approach

1. `PieterP.ScoreSheet.GUI/Views/Wizards/UpdateFromInternetPanel.xaml` (the
   panel where the province and club are picked) gets a third combo box:
   **season**.
2. `UpdateFromInternetViewModel` loads the list of seasons from
   `TabTConnector.GetSeasonsAsync()` and exposes `Seasons` /
   `SelectedSeason`.
3. The chosen `TabTSeason` is threaded through
   `UpdatingFromInternetViewModel` → `DatabaseManager.UpdateMatches` →
   `TabTUpdater.UpdateMatches`, replacing the current
   `await connector.GetActiveSeason()` call with the season the user picked.
4. The selection is persisted in `Settings.CurrentSeason` (this setting
   already exists — today it is only ever *written* after an update
   completes; from now on it is also the source of truth that other,
   secondary update paths read from instead of calling
   `GetActiveSeason()` again).
5. All the other `TabTUpdater` entry points that currently call
   `connector.GetActiveSeason()` internally (club list refresh, "new
   division day" wizard, single member-list refresh) are changed to prefer
   the stored `Settings.CurrentSeason` and only fall back to
   `GetActiveSeason()` if nothing has been stored yet (e.g. very first run).
6. `MatchTrackerService` (live away-match tracking) is changed the same way.

`TabTConnector` itself needs **no changes** — every method that can
reasonably take a season already has a `TabTSeason? season` parameter, as
you expected. The two methods that don't (`GetSeasonsAsync`,
`GetMatchSystemsAsync`) don't need one, and `GetActiveSeason()` is the one
call we are largely trying to stop relying on implicitly.

---

## 1. `SettingsDatabase` / persisted selection

**File:** `src/gui/PieterP.ScoreSheet.Model/Database/SettingsDatabase.cs`

No change needed to the schema — `Season? CurrentSeason` already exists:

```csharp
this.CurrentSeason = CreateCell<Season?>(Database.CurrentSeason, value => Database.CurrentSeason = value);
...
public Cell<Season?> CurrentSeason { get; private set; }
```

(`Season` is `PieterP.ScoreSheet.Model.Database.Entities.Season`, with
`int Id` and `string Name` — it's already persisted to `settings.ssjs` via
the underlying `AbstractDatabase<Settings>`.)

Today this cell is only ever **written to** at the end of a successful
`TabTUpdater.UpdateMatches` call (see section 4). Going forward it becomes
the persisted "selected season", written as soon as the user picks a
season in the update wizard (or, at the latest, when the update completes)
and **read** by every other place that needs a season and doesn't have a
more specific one available.

No code changes are required in this file.

---

## 2. UI: add a season drop-down to the update wizard

**File:** `src/gui/PieterP.ScoreSheet.GUI/Views/Wizards/UpdateFromInternetPanel.xaml`

Add a season selector above the existing province selector:

```xml
<StackPanel Orientation="Horizontal" Margin="0 10 0 0">
    <TextBlock Width="150" TextAlignment="Right" Margin="5" Text="{x:Static l:Resources.UpdateFromInternet_SelectSeason}"/>
    <ComboBox ItemsSource="{Binding Seasons.Value}" SelectedItem="{Binding SelectedSeason.Value}" DisplayMemberPath="Name" Width="250" Margin="5"></ComboBox>
</StackPanel>
<StackPanel Orientation="Horizontal" Margin="0 10 0 0">
    <TextBlock Width="150" TextAlignment="Right" Margin="5" Text="{x:Static l:Resources.UpdateFromInternet_SelectProvince}"/>
    <ComboBox ItemsSource="{Binding Provinces}" SelectedItem="{Binding SelectedProvince.Value}" DisplayMemberPath="Name" Width="250" Margin="5"></ComboBox>
</StackPanel>
```

(i.e. insert the new `StackPanel` right before the existing "Province"
`StackPanel`, and drop the `Margin="0 10 0 0"` that used to be on the
Province one since the new season block now carries it.)

### New localization key

Three resx files need the new key `UpdateFromInternet_SelectSeason`
(same file family as `UpdateFromInternet_SelectProvince`):

**`src/gui/PieterP.ScoreSheet.Localization/Views/Wizards/Resources.resx`**
```xml
<data name="UpdateFromInternet_SelectSeason" xml:space="preserve">
  <value>Select the season:</value>
</data>
```

**`src/gui/PieterP.ScoreSheet.Localization/Views/Wizards/Resources.nl.resx`**
```xml
<data name="UpdateFromInternet_SelectSeason" xml:space="preserve">
  <value>Selecteer het seizoen:</value>
</data>
```

**`src/gui/PieterP.ScoreSheet.Localization/Views/Wizards/Resources.fr.resx`**
```xml
<data name="UpdateFromInternet_SelectSeason" xml:space="preserve">
  <value>Sélectionnez la saison :</value>
</data>
```

(Translations are a best-effort starting point — please review before
committing.)

Optionally, also add an error string for when the season list itself
fails to download (see `UpdateFromInternetViewModel` below), e.g. a new
`Wizard_SeasonListFailed` key in `Errors.resx` / `Errors.nl.resx` /
`Errors.fr.resx`, modeled on the existing `Wizard_ClubUpdateFailed`:

```xml
<data name="Wizard_SeasonListFailed" xml:space="preserve">
  <value>An error occurred while downloading the list of seasons. You may not have internet access, or the competition server is unreachable. You can find more information about this error in the log screen.</value>
</data>
```

---

## 3. `UpdateFromInternetViewModel`: load and expose the season list

**File:** `src/gui/PieterP.ScoreSheet.ViewModels/Wizards/UpdateFromInternetViewModel.cs`

Add a `using PieterP.ScoreSheet.Connector;` (for `TabTSeason`) and
`using PieterP.Shared.Services;` is already there for `ServiceLocator`.

Add two new members and initialize them in the constructor, mirroring how
`Clubs`/`SelectedClub` are handled:

```csharp
public UpdateFromInternetViewModel(WizardViewModel parent) : base(parent) {
    this.Provinces = new ProvinceItem[] { ... }; // unchanged

    this.Seasons = Cell.Create<IEnumerable<TabTSeason>?>(null);
    this.SelectedSeason = Cell.Create<TabTSeason?>(null);
    this.SelectedSeason.ValueChanged += () => Next.RaiseCanExecuteChanged();

    this.SelectedProvince = Cell.Create(this.Provinces.First());
    this.SelectedProvince.ValueChanged += SelectedProvince_ValueChanged;
    this.Clubs = Cell.Create<IEnumerable<Club>?>(DatabaseManager.Current.Clubs.ByProvince(this.SelectedProvince.Value.Province).OrderBy(c => c.UniqueIndex));
    this.SelectedClub = Cell.Create<Club?>(null);

    // season AND club are now both required before we can continue
    this.Next = new RelayCommand(OnNext, () => SelectedClub.Value != null && SelectedSeason.Value != null);
    this.SelectedClub.ValueChanged += () => Next.RaiseCanExecuteChanged();
    this.RefreshClubs = new RelayCommand(OnRefreshClubs);
    this.IsUpdating = Cell.Create(false);

    LoadSeasons();

    if (this.Clubs.Value.Count() == 0) {
        OnRefreshClubs();
    } else {
        // select default club  (unchanged)
        ...
    }
}

private async void LoadSeasons() {
    this.IsUpdating.Value = true;
    try {
        var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
        var connectorResult = await connectorFactory.Create(true);
        if (connectorResult.Connector == null) {
            NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_SeasonListFailed, NotificationTypes.Error));
            return;
        }
        var seasons = (await connectorResult.Connector.GetSeasonsAsync())
            .OrderByDescending(s => s.Id)
            .ToList();
        this.Seasons.Value = seasons;

        // default: whatever was used last time (from settings), else the season TabT/AFTT flags as current
        var storedSeasonId = DatabaseManager.Current.Settings.CurrentSeason.Value?.Id;
        this.SelectedSeason.Value =
            seasons.FirstOrDefault(s => s.Id == storedSeasonId)
            ?? seasons.FirstOrDefault(s => s.IsCurrent)
            ?? seasons.FirstOrDefault();
    } finally {
        this.IsUpdating.Value = false;
    }
}

public Cell<IEnumerable<TabTSeason>?> Seasons { get; private set; }
public Cell<TabTSeason?> SelectedSeason { get; private set; }
```

Update `OnNext` to pass the season along:

```csharp
private void OnNext() {
    Parent.CurrentPanel.Value = new UpdatingFromInternetViewModel(Parent, SelectedClub.Value!, SelectedSeason.Value!);
}
```

Update `OnRefreshClubs` to pass the currently selected season, so the club
list itself is also fetched for the right season (club affiliations can
change from one season to the next):

```csharp
private async void OnRefreshClubs() {
    this.IsUpdating.Value = true;

    var ok = SelectedSeason.Value != null
        ? await DatabaseManager.Current.UpdateClubs(SelectedSeason.Value)
        : await DatabaseManager.Current.UpdateClubs();
    if (ok) {
        SelectedProvince_ValueChanged();
    } else {
        NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_ClubUpdateFailed, NotificationTypes.Error));
    }

    this.IsUpdating.Value = false;
}
```

(The `SelectedSeason.Value != null` guard just covers the brief window
where `RefreshClubs` could theoretically be clicked before `LoadSeasons()`
has finished; once seasons are loaded this always takes the season-aware
path.)

---

## 4. `UpdatingFromInternetViewModel`: accept and forward the season

**File:** `src/gui/PieterP.ScoreSheet.ViewModels/Wizards/UpdatingFromInternetViewModel.cs`

```csharp
using PieterP.ScoreSheet.Connector; // add

public class UpdatingFromInternetViewModel : WizardPanelViewModel {
    public UpdatingFromInternetViewModel(WizardViewModel parent, Club club, TabTSeason season) : base(parent, new CancelCommand()) {
        _club = club;
        _season = season;
        ...
    }

    public async void BeginUpdate() {
        DatabaseManager.Current.Settings.HomeClub.Value = _club.LongName;
        DatabaseManager.Current.Settings.HomeClubId.Value = _club.UniqueIndex;

        ServiceLocator.Resolve<INetworkAvailabilityService>().TriggerManually();

        if (!await DatabaseManager.Current.UpdateMatches(_club, _season, OnProgress) && !IsCanceled) {
            NotificationManager.Current.Raise(new ShowMessageNotification(Wizard_UpdateError, NotificationTypes.Error));
        }
        ...
    }

    ...
    private Club _club;
    private TabTSeason _season; // add
}
```

---

## 5. `DatabaseManager`: thread the season through

**File:** `src/gui/PieterP.ScoreSheet.Model/Database/DatabaseManager.cs`

Add `using PieterP.ScoreSheet.Connector;` and add season-aware overloads
(keep the old ones for the few callers that intentionally don't have a
specific season yet — see section 7):

```csharp
public Task<bool> UpdateClubs() {
    cancellationTokenSource = new CancellationTokenSource();
    var updater = new TabTUpdater();
    return updater.UpdateClubs(cancellationTokenSource.Token);
}
public Task<bool> UpdateClubs(TabTSeason season) {
    cancellationTokenSource = new CancellationTokenSource();
    var updater = new TabTUpdater();
    return updater.UpdateClubs(season, cancellationTokenSource.Token);
}
public Task<bool> UpdateMatches(Club club, TabTSeason season, Action<string, bool>? progressCallback = null) {
    cancellationTokenSource = new CancellationTokenSource();
    var updater = new TabTUpdater();
    if (progressCallback != null)
        updater.UpdateProgress += progressCallback;
    return updater.UpdateMatches(club, season, cancellationTokenSource.Token);
}
```

`UpdateMatches(Club, Action<string,bool>?)` (no season) can be removed
entirely, since its only caller (`UpdatingFromInternetViewModel`) now
always has a season. `RefreshMemberList(string, int)` is left as-is (see
section 7 — it keeps resolving its own season internally).

---

## 6. `TabTUpdater`: stop calling `GetActiveSeason()` for the main update flow

**File:** `src/gui/PieterP.ScoreSheet.Model/Database/Updater/TabTUpdater.cs`

### 6.1 `UpdateClubs`

Add a season-aware overload; keep the old one for backward compatibility,
routing it through a shared season-resolution helper (section 6.4) instead
of an unconditional `GetActiveSeason()` call:

```csharp
public Task<bool> UpdateClubs(CancellationToken cancellationToken) => UpdateClubs(null, cancellationToken);

public async Task<bool> UpdateClubs(TabTSeason? season, CancellationToken cancellationToken) {
    UpdateProgress?.Invoke(TabTUpdater_BeginClubUpdate, false);

    var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
    var connectorResult = await connectorFactory.Create(true);
    if (connectorResult.Connector == null)
        return false;
    var connector = connectorResult.Connector;
    if (connector.IsAnonymous)
        UpdateProgress?.Invoke(TabTUpdater_Warning, true);

    var effectiveSeason = season ?? await ResolveSeason(connector);

    var newClubs = new List<Club>();
    try {
        var clubs = await connector.GetClubsAsync(effectiveSeason);
        ...
```

(Rest of the method body is unchanged, just replace
`await connector.GetActiveSeason()` at the call site with the
`effectiveSeason` computed above.)

### 6.2 `UpdateMatches`

```csharp
public async Task<bool> UpdateMatches(Club club, TabTSeason season, CancellationToken cancellationToken) {
    // first update clubs for the SAME season, so the club list matches what we're about to download
    await UpdateClubs(season, cancellationToken); // continue, even if this fails

    UpdateProgress?.Invoke(TabTUpdater_BeginMatchUpdate, false);

    var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
    var connectorResult = await connectorFactory.Create(true);
    var connector = connectorResult.Connector;
    if (connector == null)
        return false;
    if (connector.IsAnonymous)
        UpdateProgress?.Invoke(TabTUpdater_Warning, true);

    bool everythingOk = true;

    // season is now supplied by the caller instead of always being "the active one"
    DatabaseManager.Current.Settings.CurrentSeason.Value = new Season() { Id = season.Id, Name = season.Name };
    UpdateProgress?.Invoke(Safe.Format(TabTUpdater_DownloadingSeason, season.Name), false);

    // ... rest of the method is unchanged (it already just uses the local `season` variable)
```

The only two changes here are: (1) the method signature takes `TabTSeason
season` instead of computing it via `GetActiveSeason()`, and (2) the
`UpdateClubs(cancellationToken)` call at the top now passes `season`
along, so the club list is fetched for the same season as the rest of the
update.

### 6.3 `RefreshMemberList(string clubId, int category)`, `GetDivisions(Region level)`, `GetDivisionMatches(Division division)`

These three public entry points (used by the "new division day" wizard and
by `MemberListUpdater`) are independent, occasional actions that aren't
part of the main "update from internet" wizard, so they don't get a UI for
picking a season. Instead of calling `connector.GetActiveSeason()`
directly, they should prefer whatever season was last selected/used
(persisted in `Settings.CurrentSeason`), and only fall back to
`GetActiveSeason()` if nothing has been stored yet (fresh install, before
the very first update):

```csharp
public async Task<bool> RefreshMemberList(string clubId, int category) {
    var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
    var connectorResult = await connectorFactory.Create(true, false);
    var connector = connectorResult.Connector;
    if (connector == null)
        return false;
    return await RefreshMemberList(connector, clubId, category, await ResolveSeason(connector));
}

public async Task<IList<Division>?> GetDivisions(Region level) {
    var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
    var connectorResult = await connectorFactory.Create(true);
    var connector = connectorResult.Connector;
    if (connector == null)
        return null;

    var divisionList = new List<Division>();
    try {
        var divisions = await connector.GetDivisions((TabTDivisionRegion)level, await ResolveSeason(connector));
        ...
```

```csharp
public async Task<IList<MatchStartInfo>?> GetDivisionMatches(Division division) {
    var connectorFactory = ServiceLocator.Resolve<IConnectorFactory>();
    var connectorResult = await connectorFactory.Create(true);
    var connector = connectorResult.Connector;
    if (connector == null)
        return null;

    var matchList = new List<MatchStartInfo>();
    try {
        var season = await ResolveSeason(connector);
        var matches = await connector.GetMatches(division.Id, season);
        ...
```

### 6.4 New helper: `ResolveSeason`

Add this private helper once, near the bottom of the class (next to
`LogStats`):

```csharp
/// <summary>
/// Returns the season that was last selected/used by the user (persisted in
/// Settings.CurrentSeason), falling back to the webservice's notion of the
/// "active" season only if nothing has been stored yet (e.g. first run).
/// </summary>
private static async Task<TabTSeason> ResolveSeason(IConnector connector) {
    var stored = DatabaseManager.Current.Settings.CurrentSeason.Value;
    if (stored != null)
        return new TabTSeason(stored.Id, stored.Name, false);
    return await connector.GetActiveSeason();
}
```

(`TabTSeason`'s third constructor argument, `current`, isn't used by any
of the call sites above — it only matters inside `TabTConnector
.GetActiveSeason()` itself — so `false` is fine here.)

With this in place, `NewDivisionDayCommand.cs`, `NewDivisionDayViewModel.cs`
and `MemberListUpdater.cs` need **no changes at all**: they all go through
`TabTUpdater.RefreshMemberList(string,int)` / `GetDivisions(Region)` /
`GetDivisionMatches(Division)`, which now resolve the season centrally.

---

## 7. `MatchTrackerService`: use the stored season for live away-match polling

**File:** `src/gui/PieterP.ScoreSheet.ViewModels/Services/MatchTrackerService.cs`

This service polls `GetMatchDetails` every 5 minutes for away matches of
the home club that are happening today; it currently calls
`connector.GetActiveSeason()` on every single poll, which is both an extra
round-trip and inconsistent with the rest of the app once a season is
user-selectable. Change:

```csharp
var match = await connector.GetMatchDetails(home, m.MatchId, await connector.GetActiveSeason());
```

to:

```csharp
var storedSeason = DatabaseManager.Current.Settings.CurrentSeason.Value;
var season = storedSeason != null
    ? new TabTSeason(storedSeason.Id, storedSeason.Name, false)
    : await connector.GetActiveSeason();
var match = await connector.GetMatchDetails(home, m.MatchId, season);
```

(`PieterP.ScoreSheet.Connector` is already imported in this file for
`IConnector`/`TabTSeason`, no new `using` needed.)

---

## 8. Other call sites found — informational only, no change proposed

Searched for every call to `TabTConnector`/`IConnector` members and to
`GetActiveSeason()` across the whole solution. Besides the ones already
covered above, the only remaining call sites are in debug/test code, which
is out of scope for this change (not part of the shipped application, and
already fine to keep resolving the season on the fly):

- `src/gui/DebugProject/Program.cs` — a manual test harness (`Program.cs`,
  the `DebugProject`), calls `connector.GetActiveSeason()` directly a few
  times. Not part of the shipping app; left as-is.
- `src/gui/PieterP.ScoreSheet.Tests/TabTTests.cs` — unit tests against the
  live TabT/AFTT webservice, calls `GetActiveSeason()` and various
  `Get...Async()` overloads without a season. Left as-is; these are
  exercising the connector directly, not the update flow.

`connector.TestAsync()` and `connector.UploadAsync(csv)` (used in
`MatchUploader.cs`, `LiveUpdateService.cs`, `UploadViewModel.cs`,
`ConnectorFactory.cs`) don't take a season parameter at all (correctly —
testing credentials and uploading a match result aren't season-scoped
operations), so there is nothing to change there.

`GetMatchSystemsAsync()` and `GetSeasonsAsync()` themselves are also not
season-scoped (the latter is precisely what the new season drop-down will
call), so they're unaffected too.

---

## 9. A pre-existing quirk worth knowing about (not part of this change)

`TabTConnector.GetActiveSeason()` caches its result in a **`static`** field
(`private static TabTSeason? _activeSeason = null;`), meaning that once any
part of the app calls it, the result is cached for the lifetime of the
process across *all* `TabTConnector` instances — not just the instance
that made the call. This isn't something this change needs to fix (we're
mostly moving away from calling `GetActiveSeason()` at all in the main
flows), but it's good to be aware of: `ResolveSeason()` above intentionally
never touches that cache once `Settings.CurrentSeason` has a value, which
sidesteps the issue for all the flows this document covers.

---

## 10. Suggested implementation order

1. Localization keys (section 2) — no compile dependencies.
2. `TabTUpdater` changes (section 6) — compiles standalone once the
   `ResolveSeason` helper and new overloads are in place; old
   no-season-parameter callers keep compiling against the kept overloads.
3. `DatabaseManager` changes (section 5).
4. `UpdatingFromInternetViewModel` (section 4).
5. `UpdateFromInternetViewModel` (section 3).
6. XAML (section 2).
7. `MatchTrackerService` (section 7).
8. Manual test pass: run an update for the current season (regression),
   then run one for a past season and confirm the club/match/member data
   downloaded actually reflects that season (e.g. compare against what the
   VTTL/AFTT website shows for that season), and confirm
   `Settings.CurrentSeason` (and the `OfficialMatchesPath` folder it drives)
   updates accordingly.
