# Auto-Webpage-Login-Password-by-force
A simple C# GUI interface that will allow Automatic login to a web page and can brute force the passwords on most web pages.

USAGE:
Start by enetring a valid URL.

The web scraping will be carried out while the web page is loading and can be found in seperate tabs.

The "web page input IDs" section needs to be given values found from the html web scrapping to allow this to work.

When HMTL input IDs and Button name is added the "Test" button can be pressed to check if it will work.

After one or two tries of a successfull "Test" button press a username and password can be added to the "Username & Password Login" section.

The "URL Refer" button is to be pressed to allow the current Url to be saved and is compared if a redirect will happen either due to
too many password attempts or a successfull login at this time the username and password will be added to a listbox found in the tabs sections.

The password brute forcing will found in the tabs under "Send Password from file" section and this will update the password box with values 
from your wordlist of choice and then send it to the web page if the test went ok when setting up.

If a web page is claiming the web browser is out of date there is a tab section "Upgrade Browser" will be used to 
either upgrade or roll back your browser version.

Any errors while loading will be placed in listbox section "List of Errors" for reference.

Improvements are welcome aswell as collaboration.

