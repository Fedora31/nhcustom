# nhcustom

nhcustom is a (work-in-progress) application whose purpose is to modify the Team Fortress
2 mod "no-hats-bgum". It looks for a configuration file named "config.txt" and executes
the actions specified in it.

If you want to remove specific hats from the game or if you want to remove everything
and handpick yourself the hats that you want to keep seeing, this program should come in
handy.


## Quick notice

This program has been/is being written in the aim to learn C\# and programming in
general. I **do not** have **any** experience in this field. I will continue to
improve the code whenever I have the time and the motivation. My knowledge is
slowly growing each day, please give it some time.

I'm making this program for the fun of it.


### Content:

* [Introduction](#introduction)
	* [Dependencies](#dependencies)
* [The configuration file](#the-configuration-file)
* [Exceptions (!)](#exceptions-(!))
* [Asterisks (\*)](#asterisks(%2A))
* [Comments](#comments)
* [The (KEEP) and (REMOVE) flag](#the-(keep)-and-(remove)-flag)
* [The input folder](#the-input-folder)
* [The output folder](#the-output-folder)
* [The database](#the-database)
* [Contact](#contact)
* [Configuration file example](#configuration-file-example)


## Introduction

The program needs to be in the same
directory as the config file, the database file, the input
folder and the output folder.

The program or the database are nowhere near perfect. If you encounter a proplem
(e.g. you can't make a hat (dis)appear, the resulting mod is behaving strangely),
feel free to contact me.

The first thing you have to do before trying to run this program is to go and
download an uncompressed version of no-hats-bgum. You can download an up-to-date
version here:

https://github.com/Fedora31/no-hats-bgum/blob/master/nhm\_source/mechbgum\_no\_hats\_mod/no\_hats\_bgum.zip

Place it in the "input" folder. Be sure the filepath is the same as this one: `input/models/...`


### Dependencies

This application is written in C\# with MonoDevelop.

For Windows, it will most likely work out of the box (at least it did for me)
but I cannot guarantee it. if it doesn't, it's *probably* because you don't have
a .NET framework installed. See this link:

https://dotnet.microsoft.com/download/dotnet-framework

For Linux, no dependencies are required as the executable comes bundled with
every library it needs (explaining its big filesize). It was tested on
Ubuntu 20.04 and Fedora 32.

*Info: to run the program on Linux, cd in the same directory as the program and
type "./nhcustom".*


## The configuration file

**Heads-up:** The program is CASE-SENSITIVE. Be sure that you put the exact name
of what you want in the config file. Refer to the teamfortress wiki to get
the correct names of the cosmetics or anything else you want to include in it.

Anything in the config file is what you **don't** want to see in-game.
If you want the config to have the opposite effect, you can add the "(KEEP)"
flag to your config file. Continue reading to know what I'm talking about.


**What you can specify in the configuration file:**

* The name of a hat (ex: Fancy Fedora)
* An update (ex: Scream Fortress 2013)
* An equip region (ex: Feet)
* A class (ex: Demoman)
* A date (ex: 2015 or 2015-04/2016 or 2015-05-02/2016-09-10)

*Info: if you type something like "2015/2016" in the config file, the program
will interpret it as "2015-01-01 to 2016-12-31". use "2015" if you only want to
remove this year.*


### Exceptions (!)

You can also tell the program to make exceptions for specific hats/updates/etc.
Just add a "!" at the beginning of the line. So if you wanted to remove every
hat from 2007 but keep the Fancy Fedora (came out it 2007), here's what you'd
write in the config file:

	2007
	!Fancy Fedora

Be aware that the program reads the config file line by line, so if you enter a
new line that affects the exceptions you made earlier, they will be canceled.

don't do this:

	2007
	!Fancy Fedora
	2007/2008


### Asterisks (\*)

Imagine you want to remove every cosmetics that the Scout can wear. You'd
think you only need to type "Scout" in the config file, but this isn't actually
true. The program will only remove hats that are "only" for the scout, leaving
a lot of multi-class cosmetics untouched. If you want to do this, append a "\*"
to the class, like so:

	Scout*

Now, the program will remove any cosmetics that can be worn by the scout, not
only scout exclusive ones. You would also want to remove all-class cosmetics,
as they don't refer to any class specifically in the database.

Something like this:

	Scout*
	All classes

this is a limitation of the program. I'll try to implement a way to remove
the cosmetics of a specific class in the future. There is also no way to remove with a
single line cosmetics that are worn by multiple classes, say the Scout,
Soldier and Engineer, but that are not considered "all-class" cosmetics,
since they aren't worn by every class. You would need to type each of those
hats manually. Some improvements are needed here.

*Info: Appending a "\*" also works with equip regions, but nothing else.*


### Comments

You can make comments in your config file. Add a "#" before any of them. ex:

	#Here is a comment
	2007 #You can also add them on the same line as a parameter
	#Everything written after a "#" is ignored, so don't write parameters after one.


### the (KEEP) and (REMOVE) flags

You should add one of these 2 flags in your config file to specify whether you want
the config file to contain entries that will be removed in the game ((REMOVE)), or
entries that will be kept in the game, and everything else removed ((KEEP)). Ideally,
your config file should have one (and only one) of those 2 flags in it.

The (KEEP) flag makes the program copy the input folder to the output folder,
then remove any cosmetics specified in the config from the output folder (which
means these cosmetics will **appear** in-game, and everything else **won't**).
The (REMOVE) flag simply makes the program copy the specified cosmetics to the
output folder (so they will **not** appear in-game, and everything else
**will**).

This was added because the database will never be error-free. With this method,
cosmetics that have wrong paths in the database or that are simply absent from
it (like most of the medals) will still be correctly removed when using the
(KEEP) flag.

If no flag is specified, the program will use the (REMOVE) flag as default.

Here is an example with the (KEEP) flag:

	#the flag can be anywhere in the program, but only the first one will be valid
	(KEEP)
	Fancy Fedora #This config will only make the Fancy fedora visible.
	(REMOVE) #This flag will be ignored as another flag has already been set.


## The input folder

This is where you need to place a (UNCOMPILED, so not a .vpk file) version
of the mod. The program will search for files in it and copy them over the
output folder based on the instructions of the config file.

You can decompile the mod and place it in this folder, or you can download an
already decompiled and up to date version here:

https://github.com/Fedora31/no-hats-bgum/blob/master/nhm\_source/mechbgum\_no\_hats\_mod/no\_hats\_bgum.zip

Be sure the filepath is the same as this one:
`input/models/...`


## The output folder

The resulting mod will be placed in this folder. **It is wiped clean everytime you
run the program, so make sure that you backup anything you want to keep.** If the
output folder doesn't exist, it will be created automatically.

**The resulting mod isn't compiled.** Refer to these 2 videos to know how to compile
it to a .vpk file and install it.

Windows: https://streamable.com/uav0li

Linux: https://streamable.com/vxchci


## The database

Every cosmetic is (or should be) referenced in this file, alongside every class
they are for, every equip regions they occupy, with which update they came with,
their release date, and their filepath in the game. The database may
(and probably do) contain wrong information in it (e.g wrong paths, dates,
equip regions, etc.) If you find something wrong in it, I'd be happy to
correct it.

**Note:** I am aware of the red errors that appear at the end of the program. Most
of the are from cosmetics absent from the mod (because they aren't working) but
that are still referenced in the database for the sake of it. Those cosmetics are:

* any "zombie" cosmetics
* the ghost aspect
* the last breath

Feel free to contact me if you get other errors.

The program doesn't contain hard-coded lists for updates, hats or anything, it
scans the database for everything it needs. So I guess that if you add custom
entries to it, it *should* work. I haven't tested this though.


## Contact

If you want to contact me about this program, You can reach me at this e-mail
address: `pevhs AT airmail D0T cc`

You can also contact me through steam: https://steamcommunity.com/id/mranchois/


## Configuration file example

Here is an example of a configuration file.

<pre>

#KEEP flag, so the config file contains parameters that will appear in-game.
(KEEP)

#Updates
Sniper vs. Spy Update
Very Scary Halloween Special
Classless Update
Über Update
First Community Contribution Update

#Hats
Counterfeit Billycock
!Magistrate's Mullet
Spectre's Spectacles
Sneaky Spats of Sneaking
Harmburg
Reader's Choice
Stealthy Scarf
Backwards Ballcap
Lucky No. 42
Dillinger's Duffel
Fed-Fightin' Fedora
Bigg Mann on Campus
Messenger's Mail Bag
Bottle Cap
Team Captain
Armored Authority
Soldier's Slope Scopers
Lone Survivor
Bushman's Bristles
Most Dangerous Mane
Macho Mann
Crocodile Smile
Hat With No Name
Last Straw
Trencher's Tunic
Couvre Corner
All-Father
Ze Übermensch
Self-Care
Heavy Duty Rag

#Equip regions
Disconnected Floating

</pre>