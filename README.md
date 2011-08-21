What's What
===========

IrcBot
------
Responsible for connecting to the IRC server and managing player events.

SmartIrc4Net
---------
Third-party .Net IRC library

SourceMod
---------
Contains the SourceMod plugin responsible for sending game information (wins, sub requests) to the PUG system.

SourceRcon
----------
Third-party .Net RCON library

TF2Pug
------
This is the main project. Contains all the PUG logic code. Is separated from the IrcBot to make it easy in the future to replace IRC with a web interface.

TF2PugTests
-----------
Test cases for the TF2Pug project.

TF2Pug.sql
----------
SQL script to generate the tables and stored procedures used by the PUG system.

WebBot
------
Right now only handles recording when games end. Should also handle sub requests. Future replacement for the IrcBot.