MODNAME		:= Kethane
KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
KGAMEDATA	:= ${GAMEDATA}/${MODNAME}
PLUGINDIR	:= ${KGAMEDATA}/Plugins

DLLNAME		:= ${MODNAME}Particles.dll

TARGETS		:= ${DLLNAME}

KP_FILES := \
	GradientCfg.cs \
	MinMaxCurveCfg.cs \
	MinMaxGradientCfg.cs \
	ParticleSystemCfg.cs \
	Utils.cs \
	$e

RESGEN2		:= resgen2
CSC			:= csc
CSCFLAGS	:= -highentropyva- -noconfig -nostdlib+ -t:library -optimize -optimize -debug -warnaserror
GIT			:= git
TAR			:= tar
ZIP			:= zip

#all: version ${TARGETS}
all: ${TARGETS}

.PHONY: version
version:
	@./tools/git-version.sh

info:
	@echo "${MODNAME} Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    csc:        ${CSC}"
	@echo "    gmcs flags: ${CSCFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

SYSTEM := \
	-lib:${MANAGED} \
	-r:${MANAGED}/mscorlib.dll \
	-r:${MANAGED}/System.dll \
	-r:${MANAGED}/System.Core.dll

KSP := \
	-r:Assembly-CSharp.dll \
	-r:Assembly-CSharp-firstpass.dll

UNITY := \
	-r:UnityEngine.CoreModule.dll \
	-r:UnityEngine.PhysicsModule.dll \
	-r:UnityEngine.ParticleSystemModule.dll \
	$e

${DLLNAME}: ${KP_FILES}
	${CSC} ${CSCFLAGS} ${SYSTEM} ${KSP} ${UNITY} -out:$@ $^

clean:
	rm -f ${TARGETS} *.mdb

install: all
	mkdir -p "${PLUGINDIR}"
	cp ${TARGETS} "${PLUGINDIR}"

.PHONY: all clean install
