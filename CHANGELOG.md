# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unrealeased] - 2022-08-15
### Modified
- Merge .position and .rotation into .SetPositionAndRotation for better performance

## [1.0.0] - 2022-06-15
### Added
- Add .samples.json file according to validation tools

### Fix
- Remove stray meta file in samples
- Adjust changelog line spacing such that documentation can be correctly generated



## [1.0.0-preview.4] - 2022-06-14

### Added
- HPF Sample Scene

### Modified
- SerializableDoubleBounds can be explicitly cast to Bounds

## [1.0.0-preview.3] - 2022-06-02

### Modified
- HPNode is now an abstract class instead of an interface.
- Updated the documentation with the samples scene.



## [1.0.0-preview.2] - 2022-05-20

### Added
- SerializableDoubleBounds to allow for easy DoubleBounds serialization

### Modified
- Make DoubleBounds readonly



## [1.0.0-preview.1] - 2022-05-11

### Modified
- Merged CoordinateSystemInspector.TargetTransform and CoordinateSystemInspector.TargetRoot to CoordinateSystemInspector.Target
- Removed HPNode.NodeType
- Limit the API access
- Removed Unity.Geospatial.HighPrecision.Internal namespace



## [1.0.0-preview.0] - 2022-05-11

### Added
- Add **Known Limitations** documentation.

### Modified
- Changed DBound Empty evaluation to be a bool instead of a negative value.
- Now using Unity.Mathematics
- DMatrix4x4, DVector2, DVector3, DVector4 have been removed
- Changed namespaces to Unity.Geospatial.HighPrecision

### Fix
- HPTransform not updating Transform on Reset and on Paste Component Values

## [0.3.3-preview.0] - 2021-10-03

### Fix
- Fix a bug where scale is not currently updated in HPTransform

## [0.3.2-preview.0] - 2021-09-16

### Modified
- Migrate to Standard Unity Package format
- Split High Precision Framework repo from Streaming Framework
- Migrate documentation from Sphinx to Unity DocTools

### Added
- Union Operation to Bounds
- Yamato Test integration
- Add API in HPRoot to transform points, directions, and rotations
  between universe space and world space



## [0.3.1-preview.3] - 2021-06-04

### Added
- Support for multiple cameras
- A geodetic extent object which can be reused to define
  an extent for modifiers. The extent can also be modified
  at play-time.

### Modified
- Extent modifier to use the new geodetic extent object
- Extent modifier to support multiple vertex streams
- Documentation to reflect changes made to the extent
  modifier.

### Fix
- Memory leak which was detected in previous version



## [0.3.1-preview.2] - 2021-05-15

### Added
- Support for hierarchical OGC 3DTiles tilesets
- Ability to generate HP Framework package only

### Modified
- Move 3DTile json parsing to another thread
- Split Universal decoder processing over multiple frames to
  reduce overall impact on performance
- Prioritize tiles based on the ratio between their geometric
  error and their error specification, causing closer objects
  to load first rather than the opposite in previous
  implementation.



## [0.3.1-preview.1] - 2021-05-12

### Added
- Geospatial Synchronization Context to better control execution
  time of await/async in GLTFast.

### Modified
- Make it possible to have an Camera that is located in world
  space rather than being located in Universe space.
- Make it possible for extent modifier to deal with children



## [0.3.1-preview.0] - 2021-05-10

### Added
- Universal OGC 3D Tiles loader
- Dependency on GLTFast for 3D tile loader

### Modified
- HPTransform optimizations
- Changes to the internal presenter structure to
  support children (required for GLTFast)



## [0.3.0-preview.0] - 2021-04-02

### Added
- HDRP Support via HDRP Material Factory
- UGSystem Modifier Stack
- UGExtentModifier to remove or contain dataset extents
- HDRP Test project (dev only)
- ECEF Coordinate system inspector
- Force-Lit texture support on Cesium-Native, allowing unlit datasets to
  be pushed into lit shaders for relighting
- Presenter concept has been added to UGSystem, allowing datasets to 
  be spawned on different render layers as well as on different
  GameObjects.

### Modified
- Significant changes to documentation
- Moved away from .unitypackage and into package manager workflow
- UGSystem has become UGSystemBehaviour. Similar name changes have
  been made across the API for a uniform naming convention and
  preparation for future DOTS support.
- Changed the way sample datasets are loaded via package manager
- Import through package manager rather than .unitypackage
- UGSystem now uses a node graph for it's processing across multiple
  internal modules. This standardizes the way processing is done within
  the system.
- HPTransform coordinate system is now saved to editor preference rather
  than static variables, ensuring coherence from edit to play mode and from
  HPTransform to HPRoot and vice-versa.
- Geodetic coordinate system inspector now shows pitch, yaw and roll rather
  than heading, pitch, roll. Order has been changed to fit with Euler angles
  and nomenclature has been standardized to something more familiar.
- Build script has been cleaned up (dev only)
- Demo application has been migrated to HDRP
- UGSystem configuration errors will show a single explicit error message
  rather than pop an undescript message on every frame. Cesium Native error
  messages are still non-descript.
- Implemented proper error message on missing Cesium-Ion token






## [0.2.1-preview.0] - 2021-02-25

### Fixed
- Perpetual dialog that opened up when adding an HPTransform
  to a prefab.




## [0.2.0-preview.0] - 2021-02-22

### Modified
THIS IS A COMPLETELY NEW VERSION OF THE API THAT INCORPORATES
A HIGH PRECISION RENDERING FRAMEWORK. SEE BELOW FOR PRIMARY
CHANGES:
- The UGCamera must now be a child of the UGSystem
- The UGCamera must now have an HPTransform component
- The UGSystem must now have an HPRoot component
- The UGSkybox must now be a child of the UGSystem
- The UGSkybox must now have an HPTransform component
- Static rebasing is no longer implemented by the UGSystem, rather
   it is implemented via the HPRoot. This allows for dynamic
   rebasing as well as static rebasing, via the HPRoot/HPTransform.
- All objects instantiated by the UGSystem now have an HPTransform
  component.




## [0.1.0-preview.5] - 2020-12-15

### Added
- Docs: Doxygen + Breathe for API docs
- Docs: Wrote API docs for primary classes
- Makefile: Automated zip archive for builds
- Makefile: Added Unity Tests to build pipeline

### Modified
- Fly Camera script has been modified such that it is no possible
   to disable the automatic clip plane adjustements
- Build Preprocessor populates required shaders in build
- Terrain shaders had a Red Color that was shown when textures are
   not loaded. This was intended for debugging issues during development,
   not in a release. A UNITY_GEOSPATIAL_DEBUG define has been added
   to enable or disable this feature. Since new projects do not have
   this define, a black color will be shown instead, by default.

### Fixed
- Fixed bug where a light in b3dm file would cause the application
   to crash.
- Fly Camera script has been fixed such that it is no longer possible
   for the camera to get stuck looking down.




## [0.1.0-preview.4] - 2020-11-07

### Added
- Docs: Added license file to unitypackage
- Docs: Added license file in root of build product
- Docs: Added page to guide use of multiple layers

### Modified
- Fly Camera now controls near clip plane and far clip plane automatically
- Refactored some private variables in Fly Camera to match coding standard



## [0.1.0-preview.2] - 2020-11-03

### Added
- Docs: Screenshots for more explicit documentation
- Docs: How to disable fog in Getting Started

### Changed
- Changed project file structure
- Updated Makefile accordingly
- Built documentation into the Makefile



## [0.1.0-preview.1] - 2020-11-02

### Added
- Initial release for internal review
