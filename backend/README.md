## Backend

### OverviewQuery API endpoints

When the user selects the desired overview of their Time Registrations, a set of parameters are sent to the backend that then queries the database for the needed time registrations.
Based on the data gathered from the database the backend will instantiate a WorkDay[] which itself contains a DateTime property and a TimeRegistration[], which ultimately are sent back to the frontend.

#### Params and URL table

| params                                                                                                                                                                                                                                                             | default params                  | URL (in sections) api/overview | URL with default params api/overview |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------- | ------------------------------ | ------------------------------------ |
| Sort asc : bool                                                                                                                                                                                                                                                    | Sort asc = false                | ?sort_asc=(bool)               | ?sort_asc=false                      |
| Show days /w no records : bool                                                                                                                                                                                                                                     | Show days /w no records = false | &show_days_no_records=(bool)   | &show_days_no_records=false          |
| Set default : bool                                                                                                                                                                                                                                                 | Set default = false             | &set_default=(bool)            | &set_default=false                   |
| StartDate : date (if not custom then date = today)                                                                                                                                                                                                                 | StartDate = today               | &start_date=(date)             | &start_date=(today)                  |
| EndDate : date? (if not custom then date = null)                                                                                                                                                                                                                   | EndDate = null                  | &end_date=(date?)              | &end_date=null                       |
| TimePeriod (day, week, month, year) : string <br>\- "day" for day<br>\- "week" for week<br>\- "month" for month<br>\- "year" for year<br>\- "custom" for custom period                                                                                             | Time Period = "week"            | &time_period=(string)          | &time_period=week                    |
| TimeMode (last, current, rolling) : string <br>\- "last" for last TimePeriod<br>\- "current" for current TimePeriod<br>\- "rolling" for rolling TimePeriod<br>\- "custom" if custom TimePeriod has been chosen                                                     | Time Mode = "current"           | &time_mode=(string)            | &time_mode="current"                 |
| Group by : string <br>\- "client" for Client<br>\- "period" for Period                                                                                                                                                                                             | Group by = "period"             | &group_by=(string)             | &group_by=periods                    |
| Then by (Only one can be selected but all can remain unselected)                                                                                                                                                                                                   | Then by = "clients"             | &then_by=(string)              | &then_by=clients                     |
| Subperiods : string <br>\- "day" for day<br>\- "week" for week<br>\- "month" for month<br>\- "consultants" for consultants<br>\- "clients" for clients<br>\- "allregistrations" for all registrations<br>\- "none" for when not selecting any "then by" Subperiods |                                 |                                |                                      |
| \*Consultants<br>Only selectable if Group By="client"                                                                                                                                                                                                              |                                 |                                |                                      |
| \*Clients<br>Only selectable if Group By="period"                                                                                                                                                                                                                  |                                 |                                |                                      |
|                                                                                                                                                                                                                                                                    |                                 |                                |                                      |

### API Output

#### Output

```json
{
    "querystring": "start_date=<DateTime.Date>, end_date=<DateTime.Date?>, time_period=<string>, time_mode=<string>, groupby=<string>, thenby=<string>, sort_asc=false, show_days_no_records=false, set_default=false",
    "time_mode_period": {
        "name": "<time_mode> <time_period>",
        "total": "<int>"
    },
    "<time_period>": {
        "<groupby>": [{
            "<grouby-object>": {
                "name": "<string>",
                "total": "<int>",
                "<thenby>": [{
                    "<thenby-object>": {
                        "name": "<string>",
                        "total": "<int>"
                    }
                    ...
                }]
            }
            ...
        }]

    }
}
```

#### Default output

```json
{
  "querystring": "start_date=DateTime.Today, end_date=null, time_period=week, time_mode=current, groupby=period, thenby=clients, sort_asc=false, show_days_no_records=false, set_default=false",
  "period": {
    "name": "current week",
    "timespan": "monday <date> - friday <date>",
    "total": "<int>"
  },
  "week": {
    "days": [
      {
        "day": {
          "name": "<DayOfWeek> + <date>",
          "total": "<int>",
          "clients": [
            {
              "client A": {
                "name": "client A",
                "total": "<int>"
              },
              "client B": {
                "name": "client B",
                "total": "<int>"
              }
            }
          ]
        },
        "day": {
          "total": "<int>",
          "clients": [
            {
              "client A": {
                "name": "client A",
                "total": "<int>"
              }
            }
          ]
        },
        "day": {
          "total": "<int>",
          "clients": [
            {
              "client B": {
                "name": "client B",
                "total": "<int>"
              }
            }
          ]
        },
        "day": {
          "total": "<int>",
          "clients": [
            {
              "client B": {
                "name": "client B",
                "total": "<int>"
              },
              "client D": {
                "name": "client D",
                "total": "<int>"
              }
            }
          ]
        },
        "day": {
          "total": "<int>",
          "clients": [
            {
              "client A": {
                "name": "client A",
                "total": "<int>"
              }
            }
          ]
        }
      }
    ]
  }
}
```
