Windows Utils
===
Some simple utilities for daily use.

The projects in this solution are independent from each other, and each of them can be safely removed.

## NoAdmin

`noadmin path [args]`
The program sets environment variable \_\_COMPAT_LAYER to RUNASINVOKER and launch given program.

## RunAsAdmin

`runasadmin path [args]`

Run given program as administrator.

## DoNothing

Simply do nothing and exit.

This program is used to replace unwanted executables in other softwares.

## WaitAndDoNothing

Open a console window, and wait until a user closes it.

## WUClean

Clean up the system drive after windows update.

Warning: This is a very slow operation, after which you will not be able to uninstall the update. `C:\Windows\SoftwareDistribution\Download` is removed afterward, which has a small chance of causing some problems or even breaking your system.

## csps / cspsc

```
csps [OPTIONS] FILE [ARGS]
csps [OPTIONS] /c FILE [ARG1 ARG2 ...]
```

Start a process with various options.

For more information, see `cspsc /?`.

## CallSHChangeNotify

Call Windows API function SHChangeNotify in command line.

## GetInterfaceIP

Get an IP address from a given net interface name.
