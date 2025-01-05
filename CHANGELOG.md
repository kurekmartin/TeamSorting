### New
- add search when editing with/not with values 
- add ability to delete all input data
- add CSV validation with new dialog to show errors during loading 
- add confirmation after CSV export of resulting teams

### Changed
- reworked UI for editing with/not with
  - it is now possible to select and deselect multiple members without the popup closing  
- discipline values now uses custom UI controls instead of text
- changed UI for sorting resulting teams
- add validation of member name (empty name, duplicate values)
- when editing with/not with updated also other affected members
- with/not with is now sorted by name 
- decimal separator now uses system settings
- [CSV] time values now have custom format hh:mm:ss.f
  - hours, minutes and fractional seconds are optional
- [CSV] with and not with columns are optional

### Fix
- update member names inside with/not with when renaming member
- with/not with updates after member is deleted
- sorting resulting teams was not updating correctly after changing sorting criteria
- fix ability to resize warning dialogs
