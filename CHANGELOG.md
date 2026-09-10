### New
- add support for renaming teams with validation for empty and duplicate names
- add team action menus, deleting all teams, and toggling unsorted members
- add support for switching between light and dark themes
- add undo and redo for adding or deleting teams and moving members between teams
- add keyboard navigation and single-click editing to the input table
- validate conflicts between member "With" and "Not with" constraints

### Changed
- reorganize input and team actions and improve their labels and layout
- automatically show or hide unsorted members
- improve sorting and value calculation performance 
- change dialogs to show as overlays
- fix memory leaks when switching views
- show error message when trying to add members or disciplines with duplicate names

### Fixed
- update sorting when a value for the active sorting criterion changes
- incorrect team capacity in certain scenarios
- potential out-of-range errors during sorting
- slow sorting for disciplines with a zero value range
