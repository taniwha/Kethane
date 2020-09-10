KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
KGAMEDATA  := ${GAMEDATA}/Kethane
PLUGINDIR	:= ${KGAMEDATA}/Plugins

TARGETS		:= Kethane.dll

K_FILES := \
	BodyResourceData.cs	\
	Generators/CellularResourceGenerator.cs	\
	Generators/LegacyResourceGenerator.cs	\
	Generators/RandomExtensions.cs	\
	IResourceGenerator.cs	\
	KethaneController.cs	\
	KethaneData.cs	\
	Misc.cs	\
	Particles/ParticleManager.cs \
	Particles/ParticlePhysics.cs \
	PartModules/HeatSinkAnimator.cs	\
	PartModules/IDetectorAnimator.cs	\
	PartModules/IExtractorAnimator.cs	\
	PartModules/KethaneConverter.cs	\
	PartModules/KethaneDetector.cs	\
	PartModules/KethaneDetectorAnimator.cs	\
	PartModules/KethaneDetectorAnimatorUnity.cs	\
	PartModules/KethaneDrillAnimator.cs	\
	PartModules/KethaneDrillAnimatorLegacy.cs	\
	PartModules/KethaneExtractor.cs	\
	PartModules/KethaneGenerator.cs	\
	PartModules/KethaneKerbalBlender.cs	\
	PartModules/KethaneParticleEmitter.cs	\
	PartModules/KethaneWetMassIndicator.cs	\
	PartModules/OrthogonalIntake.cs	\
	PartModules/PartExtensions.cs	\
	PartModules/TimedMovingAverage.cs	\
	Properties/AssemblyInfo.cs	\
	ResourceData.cs	\
	ResourceDefinition.cs	\
	Scenarios/KethaneScanningTutorial.cs	\
	SettingsManager.cs	\
	UserInterface/KerbNetModeKethane.cs	\
	UserInterface/MainMenuOverlay.cs	\
	UserInterface/MapOverlay.cs	\
	UserInterface/OverlayRenderer.cs	\
	UserInterface/TerrainData.cs	\
	Utilities/CompatibilityChecker.cs	\
	Utilities/InstallChecker.cs	\
	Utilities/InstallCleanup.cs	\
	Utilities/KopernicusWrapper.cs	\
	Utilities/LicenseSentinel.cs	\
	Utilities/ShaderLoader.cs \
	Utilities/TutorialInstaller.cs	\
	VesselModules/IKethaneBattery.cs \
	VesselModules/KethaneBattery.cs \
	VesselModules/KethaneProtoBattery.cs \
	VesselModules/KethaneProtoDetector.cs \
	VesselModules/KethaneScanner.cs \
	$e

RESGEN2		:= resgen2
CSC			:= csc
CSCFLAGS	:= -highentropyva- -noconfig -nostdlib+ -t:library -optimize -debug -warnaserror+
GIT			:= git
TAR			:= tar
ZIP			:= zip

#all: version ${TARGETS}
all: ${TARGETS}

.PHONY: version
version:
	@./tools/git-version.sh

info:
	@echo "Kethane Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${CSC}"
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
	-r:Assembly-CSharp-firstpass.dll \
	-r:KSPAssets.dll

UNITY := \
	-r:UnityEngine.dll \
	-r:UnityEngine.UI.dll \
	-r:UnityEngine.CoreModule.dll \
	-r:UnityEngine.AnimationModule.dll \
	-r:UnityEngine.PhysicsModule.dll \
	-r:UnityEngine.IMGUIModule.dll \
	-r:UnityEngine.AudioModule.dll \
	-r:UnityEngine.ParticleSystemModule.dll \
	-r:UnityEngine.TextRenderingModule.dll \
	-r:UnityEngine.InputLegacyModule.dll \
	-r:UnityEngine.UnityWebRequestWWWModule.dll \
	-r:UnityEngine.UnityWebRequestModule.dll \
	-r:UnityEngine.AssetBundleModule.dll \
	$e

Kethane.dll: ${K_FILES} Kethane-LICENSE.txt
	${CSC} ${CSCFLAGS} ${SYSTEM} ${KSP} ${UNITY} \
		-lib:../../../GeodesicGrid/bin -r:GeodesicGrid.dll \
		-lib:../Particles -r:KethaneParticles.dll \
		-resource:Kethane-LICENSE.txt,Kethane.Kethane-LICENSE.txt \
		-out:$@ ${K_FILES}

clean:
	rm -f ${TARGETS}

install: all
	mkdir -p "${PLUGINDIR}"
	cp ${TARGETS} "${PLUGINDIR}"
#cp ${TARGETS} bin/Kethane.version ${PLUGINDIR}

.PHONY: all clean install
