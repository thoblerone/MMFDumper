
MMFDumper
====

MMFDumper is a command line utility that can be used to create a hex/text dump of a memory mapped file.

Usage:

MmfDumper filename [/maxLength=(0|length)]
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;filename=name of file in MMF namespace, e.g. Global\windows_shell_global_counters 
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;/maxLength=0: dump whole file contents 
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;/maxLength=1024: display first 1024 bytes (multiples of 16)
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;omitted maxLength parameter uses 256 bytes


Copyright (c) 2019 thoblerone@freenet.de



ToDo
----
better formatting if maxLength is used with a length that is not a multiplyer of 16
