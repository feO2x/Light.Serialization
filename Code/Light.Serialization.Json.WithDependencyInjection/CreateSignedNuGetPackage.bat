copy bin\Release\Light.Serialization.Json.WithDependencyInjection.dll bin\SignedRelease\ /Y
copy bin\Release\Light.Serialization.Json.WithDependencyInjection.xml bin\SignedRelease\ /Y

ildasm bin\SignedRelease\Light.Serialization.Json.WithDependencyInjection.dll /out:bin\SignedRelease\Light.Serialization.Json.WithDependencyInjection.il
del bin\SignedRelease\Light.Serialization.Json.WithDependencyInjection.dll
ilasm bin\SignedRelease\Light.Serialization.Json.WithDependencyInjection.il /dll /key=bin\SignedRelease\Light.Serialization.Json.WithDependencyInjection.snk
del bin\SignedRelease\Light.Serialization.Json.WithDependencyInjection.il
del bin\SignedRelease\Light.Serialization.Json.WithDependencyInjection.res

nuget pack Light.Serialization.Json.WithDependencyInjection.nuspec