### New
- add support for drag&drop to move members between teams
- show information about team changes after member was moved  
- add ability to manually sort members into teams
- add ability to create a new combination directly from teams view
- add progress indication during sorting
- add discipline averages to teams view

### Changed
- completely reworked displaying of input data
  - changed from DataGrid to TreeDataGrid
  - this should lead to better performance when inputting data and scrolling 
- add ability to switch back and forth between input view and teams view without deleting data
  - this allows you to edit input data without losing current teams
- discipline values are now normalized during sorting
  - this should lead to more balanced teams 
- discipline values are now formatted based on discipline data type

### Fix
- fix duplicate border on dialogs