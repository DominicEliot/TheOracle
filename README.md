## About
TheOracle2 is a complete rework of TheOracle using new discord bot features, and more integrated data features.

## Joining the bot to your discord server
To add TheOracle to a discord server click [this link](https://discord.com/api/oauth2/authorize?client_id=704480988561932389&permissions=431644532800&scope=bot%20applications.commands) and then select the server you wish to add the bot to.

If you need a discord server for your game you can use [this link](https://discord.new/hevebmEhcjCa) to get started with a preexisting discord server template.

## Getting started with bot commands
1. If you want to get content for a game other than Ironsworn (Starforged or Sundering Isles) set your game with `/set-game`
2. Create a character with `/player-character create`
3. Add some assets with `/asset`
4. Roll some actions with `/roll pc-action`
5. Get the results from an oracle with `/oracle`
6. View all the commands available in the bot by typing `/` and scrolling though the options (if you have a lot of bots joined to your server you can click TheOracle bot's icon to show only its commands)

## Other features
#### Recreate message:
You can recreate a message, similar to the old ⏬ reaction method by right clicking a bot message and selecting recreate message from the apps menu.

![image](https://user-images.githubusercontent.com/6792312/147948167-a1b67087-5064-40e4-b4e5-9f3738ade82a.png)

## Running the bot yourself
note: This is for people that want to change the source code and run their own instance of the bot. It's not something most users will want/need to do.
* Install PostgreSQL Server
* Create a new database and db user for the bot to use
* Create a database settings file named `dbSettings.json` so the bot knows how to connect. It should have a structure similar to this:
```
{
    "dbConnectionString":"Host=localhost;Port=5432;Database=NameOfDbYouCreated;Username=BotDbUser",
    "dbConnectionStringWithPort":"host=127.0.0.1 port=5432 dbname=NameOfDbYouCreated connect_timeout=10 user=BotDbUser",
    "dbConnectionStringOff":"postgresql://BotDbUser@localhost:5432/NameOfDbYouCreated",
    "dbPassword":"YourDbUserPassword"
}
```
* Get a discord bot token from the discord developer portal
* Start the bot server, paste your token when prompted. (If you need to change the token it's stored in the token.json file in your server's folder)

## AI generated code
This project was originally written without the use of AI generated code, but user contributions are allowed to use AI tools to generate code and documentation. All the code and documentation in this project is reviewed by humans before being approved.

### AI generated contributions:
I recognize that AI assisted coding and fully generated AI code is becoming more and more common for developers. It can improve productivity, and help people make changes to the project more quickly. Unfortunately AI generated code often has subtle bugs, can be hard to follow, and can create additional work for project maintainers. This project allows contributions to be made that are generated using AI tools, but low effort PRs that appear to be entirely created with AI maybe be closed at anytime.

When submitting a PR using AI generated content:
* You must be open with the use AI generated content, and summarize what was generated
* Match the style and conventions of the project
* You own the changes, meaning that you will be responsible for debugging, testing, and possibly fixing any issues
    * A PR is a collaborative learning experience, if there are parts you don't understand be upfront with them in the PR comments
* Watch for AI's tendency to generate code with verbose/unnecessary comments, and unnecessary unit tests

## Privacy
TheOracle bot doesn't store any user data of any kind, except for commands that are explicitly handled by the bot. Any data collected will not be sold or used for anything other than further developing and improvement of the bot.
