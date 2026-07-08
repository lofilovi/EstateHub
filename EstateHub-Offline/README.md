# EstateHub - Offline Demo

This is a fully self-contained copy of the EstateHub dashboard. No .NET, no MySQL, no server, no download of a database - just one HTML file.

## How to run it

Double-click `index.html`. It opens directly in your browser and works immediately.

## How it works

Every part of the UI is identical to the full app, but instead of talking to a real ASP.NET Core + MySQL backend, all API calls are intercepted in the browser and served from an in-memory "fake database" seeded with the same demo data (properties, apartments, tenants, calendar events, work orders, inspections, accounting history, etc.).

Anything you add, edit, or delete (apartments, tenants, calendar events, inspection reports, settings, your profile...) is saved to your browser's `localStorage`, so it's still there next time you open the file - but it only lives in your own browser, on your own machine. Nothing is sent anywhere.

## Default login

If you click **Log Out** in the sidebar and want to sign back in:

- **Email:** `viktor@estatehub.com`
- **Password:** `admin123`

## Resetting the demo data

If you want to start fresh with the original demo data again, open your browser's DevTools console on this page and run:

```js
localStorage.removeItem('estatehub_offline_data_v1');
location.reload();
```
