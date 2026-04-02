# Service Alarms

## About

This data source is designed to provide information about active alarms within a service. It will retrieve a row for each active alarm present in the service.

![Query builder](./Images/QueryBuilder.png)

## Input/output

- Input: Service name
- Output: Information about each of the active alarms of the specified service:

  - ID (String)
  - Element (String)
  - Parameter (String)
  - Value (String)
  - Time (DateTime)
  - Severity (String)
  - Owner (String)

## Use Cases

You can use this data source in a dynamic context by linking its input to data in dashboards or low-code apps to do the following:

- Monitor active alarms for a specific service.
- Display alarm information in dashboards with real-time updates.
- Filter and analyze service alarms by severity, owner, or time.
