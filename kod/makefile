#
# Makefile for compiling the Blakod. This is the 'main' makefile, so kod.mak
# is reproduced here with some extra commands to run after the makefile
# recurse is done (notably creating the client rsb file). We need to do this
# here instead of the root directory makefile in case anyone builds the blakod
# from this directory.
#

TOPDIR=..
!include $(TOPDIR)\common.mak

BOFS = util.bof object.bof

.SUFFIXES : .kod

BCFLAGS = -d -I $(KODINCLUDEDIR) -K $(KODDIR)\kodbase.txt

.kod.bof::
	@$(BC) $(BCFLAGS) $<
	@for %i in ($<) do @$(KODDIR)\bin\instbofrsc $(TOPDIR) $(BLAKSERVRUNDIR) %i

all : $(BOFS)
	cd ..\resource & nmake
	@for %i in ($(BOFS:.bof=.kod)) do @if NOT EXIST %i (echo error: Missing kod file %i with makefile entry!)

	@for %i in ($(BOFS:.bof=)) do @if EXIST %i (echo Building %i & cd %i & $(MAKE) /$(MAKEFLAGS) TOPDIR=..\$(TOPDIR) & cd ..)
	@echo Copying kodbase.txt and kod include files
	-@$(CP) $(KODDIR)\kodbase.txt $(BLAKSERVRUNDIR) 2>&1
	-@$(CP) $(KODDIR)\include\*.khd $(BLAKSERVRUNDIR) 2>&1
	@echo Copying .rsc files, creating server and client .rsb files
	@-$(RM) $(BLAKSERVRUNDIR)\rsc\*.rsb >nul 2>&1
	@-$(RM) $(CLIENTRUNDIR)\resource\*.rsb >nul 2>&1
	-@$(RSCMERGE) $(BLAKSERVRUNDIR)\rsc\rsc0000.rsb $(BLAKSERVRUNDIR)\rsc\*.rsc
	-@$(CP) $(BLAKSERVRUNDIR)\rsc\rsc0000.rsb $(CLIENTRUNDIR)\resource\ 2>&1

$(BOFS): $(DEPEND)

clean :
	@-$(RM) *.bof *.rsc kodbase.txt >nul 2>&1
	@-for %i in ($(BOFS:.bof=.)) do @if EXIST %i (cd %i & $(MAKE) /$(MAKEFLAGS) TOPDIR=..\$(TOPDIR) clean & cd .. )
	@-$(RM) $(BLAKSERVRUNDIR)\rsc\*.rsc >nul 2>&1
	@-$(RM) $(BLAKSERVRUNDIR)\loadkod\*.bof >nul 2>&1
	@-$(RM) $(BLAKSERVRUNDIR)\memmap\*.bof >nul 2>&1

