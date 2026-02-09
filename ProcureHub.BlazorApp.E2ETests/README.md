
# Debugging

To debug the tests, edit the `ProcureHub.BlazorApp.E2ETests/e2e.runsettings` file and uncomment:
- `<PWDEBUG>1</PWDEBUG>`
- `<ExpectTimeout>0</ExpectTimeout>`

This will fire up a Chrome instance, where you can inspect network tab etc. More info: https://playwright.dev/dotnet/docs/running-tests#debugging-tests.

You can also use the trace viewer: https://playwright.dev/dotnet/docs/trace-viewer-intro 
