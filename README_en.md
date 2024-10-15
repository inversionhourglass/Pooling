# Pooling

[中文](https://github.com/inversionhourglass/Pooling/blob/master/README.md) | English

Pooling is a compile-time object pool component. It simplifies the coding process by replacing the `new` operation with object pool operations at compile-time, removing the need for developers to manually write object pool operation code. It also offers a fully non-intrusive solution that can be used for temporary performance optimizations or for optimizing performance in legacy projects.

## Quick Start

Reference Pooling.Fody:
> dotnet add package Pooling.Fody

Ensure that `FodyWeavers.xml` is configured with Pooling. If the current project does not have a `FodyWeavers.xml` file, simply compile the project, and the file will be automatically generated:
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Pooling /> <!-- Ensure the Pooling node is present -->
</Weavers>
```

```csharp
// 1. The type to be pooled must implement the IPoolItem interface
public class TestItem : IPoolItem
{
    public int Value { get; set; }

    // This method resets the instance state when the object is returned to the pool
    public bool TryReset()
    {
        return true;
    }
}

// 2. Use the new keyword to create an object of this type anywhere
public class Test
{
    public void M()
    {
        var random = new Random();
        var item = new TestItem();
        item.Value = random.Next();
        Console.WriteLine(item.Value);
    }
}

// Post-compilation code
public class Test
{
    public void M()
    {
        TestItem item = null;
        try
        {
            var random = new Random();
            item = Pool<TestItem>.Get();
            item.Value = random.Next();
            Console.WriteLine(item.Value);
        }
        finally
        {
            if (item != null)
            {
                Pool<TestItem>.Return(item);
            }
        }
    }
}
```

## IPoolItem

As shown in the [Quick Start](#quick-start) section, any type that implements the `IPoolItem` interface is considered a pooled type. Pooling replaces its `new` operations with object pool operations during compilation, and returns the pooled object instance to the pool in the `finally` block. The `IPoolItem` interface only has one method, `TryReset`, which is used to reset the state when the object is returned to the pool. If `TryReset` returns false, it indicates that the state reset has failed, and the object will be discarded.

### PoolingExclusiveAttribute

By default, a pooled type that implements `IPoolItem` will be pooled in all methods of the current assembly. However, there may be cases where pooling is not desired in certain types or methods, such as for manager types or builder types for pooled objects. In such cases, the `PoolingExclusiveAttribute` can be applied to specify where pooling should not occur.

```csharp
[PoolingExclusive(Types = [typeof(TestItemBuilder)], Pattern = "execution(* TestItemManager.*(..))")]
public class TestItem : IPoolItem
{
    public bool TryReset() => true;
}

public class TestItemBuilder
{
    private readonly TestItem _item;

    private TestItemBuilder()
    {
        // Since TestItemBuilder is excluded by the PoolingExclusive's Types attribute, the following will not be replaced with pooling operations
        _item = new TestItem();
    }

    public static TestItemBuilder Create() => new TestItemBuilder();

    public TestItemBuilder SetXxx()
    {
        // ...
        return this;
    }

    public TestItem Build()
    {
        return _item;
    }
}

public class TestItemManager
{
    private TestItem? _cacheItem;

    public void Execute()
    {
        // Since all methods of TestItemManager are excluded by the PoolingExclusive's Pattern attribute, the following will not be replaced with pooling operations
        var item = _cacheItem ?? new TestItem();
        // ...
    }
}
```

As shown in the code above, the `PoolingExclusiveAttribute` has two properties: `Types` and `Pattern`. `Types` is an array of `Type` objects, specifying which types should not be pooled in their methods. `Pattern` is a string-based AspectN expression that can match specific methods (for more details on AspectN expressions, visit: https://github.com/inversionhourglass/Shared.Cecil.AspectN/blob/master/README.md). You can use either one of these properties or both together. If both are used, pooling will be excluded for all types/methods matched by either property.

## NonPooledAttribute

The `PoolingExclusiveAttribute` allows you to specify where pooling operations should not occur, but it needs to be directly applied to the pooled type. If you are using a pooled type from a third-party library, you may not have direct access to apply `PoolingExclusiveAttribute`. In such cases, you can use the `NonPooledAttribute` to indicate that pooling should not be used in the current method.

```csharp
public class TestItem1 : IPoolItem
{
    public bool TryReset() => true;
}
public class TestItem2 : IPoolItem
{
    public bool TryReset() => true;
}
public class TestItem3 : IPoolItem
{
    public bool TryReset() => true;
}

public class Test
{
    [NonPooled]
    public void M()
    {
        // Since the method is marked with NonPooledAttribute, the following new operations will not be replaced with pooling operations
        var item1 = new TestItem1();
        var item2 = new TestItem2();
        var item3 = new TestItem3();
    }
}
```

Sometimes, you may want to exclude specific pooled types from pooling, rather than all pooled types in the method. In such cases, you can use the `Types` and `Pattern` properties of the `NonPooledAttribute` to specify which pooled types should not be pooled. `Types` is an array of `Type` objects, specifying which types should not be pooled in the current method. `Pattern` is a string-based AspectN expression that matches types to exclude from pooling.

```csharp
public class Test
{
    [NonPooled(Types = [typeof(TestItem1)], Pattern = "*..TestItem3")]
    public void M()
    {
        // TestItem1 is excluded from pooling by the Types property, and TestItem3 is excluded by the Pattern property. Only TestItem2 will be pooled.
        var item1 = new TestItem1();
        var item2 = new TestItem2();
        var item3 = new TestItem3();
    }
}
```

AspectN expressions are highly flexible and support the logical NOT operator (`!`). This allows you to easily limit pooling to a specific type. For example, the above can be simplified to `[NonPooled(Pattern = "!TestItem2")]`. For more details on AspectN expressions, visit: https://github.com/inversionhourglass/Shared.Cecil.AspectN/blob/master/README.md.

The `NonPooledAttribute` can be applied not only at the method level but also at the class or assembly level. When applied to a class, it affects all methods of that class (including properties and constructors). When applied to an assembly, it affects all methods in the current assembly (including properties and constructors). Additionally, if neither `Types` nor `Pattern` is specified when applied to an assembly, it effectively disables pooling for the entire assembly.

## Non-Intrusive Pooling

The `IPoolItem` approach requires manual code changes for integration, making it suitable for new projects or small-scale modifications. However, for large legacy projects, such changes may be too risky or difficult to implement. To address this, Pooling offers a non-intrusive integration method that does not require implementing the `IPoolItem` interface, allowing developers to specify pooled types through configuration alone.

For instance, consider the following code:

```csharp
namespace A.B.C;

public class Item1
{
    public object? GetAndDelete() => null;
}

public class Item2
{
    public bool Clear() => true;
}

public class Item3 { }

public class Test
{
    public static void M1()
    {
        var item1 = new Item1();
        var item2 = new Item2();
        var item3 = new Item3();
        Console.WriteLine($"{item1}, {item2}, {item3}");
    }

    public static async ValueTask M2()
    {
        var item1 = new Item1();
        var item2 = new Item2();
        await Task.Yield();
        var item3 = new Item3();
        Console.WriteLine($"{item1}, {item2}, {item3}");
    }
}
```

After adding a reference to `Pooling.Fody` in your project, a `FodyWeavers.xml` file will be generated in your project folder. Modify the `Pooling` node as follows:

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Pooling>
    <Items>
      <Item pattern="A.B.C.Item1.GetAndDelete" />
      <Item pattern="Item2.Clear" inspect="execution(* Test.M1(..))" />
      <Item stateless="*..Item3" not-inspect="method(* Test.M2())" />
    </Items>
  </Pooling>
</Weavers>
```

In the configuration, each `Item` node represents a pooled type, with four attributes:

- **pattern**: An AspectN-style type+method expression that specifies a pooled type and its reset method (equivalent to `IPoolItem.TryReset`). The reset method must be parameterless.
- **stateless**: An AspectN-style type expression. This specifies a stateless pooled type that can be returned to the pool without reset operations.
- **inspect**: An AspectN expression specifying methods in which pooled types can be used. When omitted, it applies to all methods in the current assembly.
- **not-inspect**: An AspectN expression that excludes specific methods from pooling. When omitted, no methods are excluded.

With the above configuration, the compiled `Test` class becomes:

```csharp
public class Test
{
    public static void M1()
    {
        Item1 item1 = null;
        Item2 item2 = null;
        Item3 item3 = null;
        try
        {
            item1 = Pool<Item1>.Get();
            item2 = Pool<Item2>.Get();
            item3 = Pool<Item3>.Get();
            Console.WriteLine($"{item1}, {item2}, {item3}");
        }
        finally
        {
            if (item1 != null)
            {
                item1.GetAndDelete();
                Pool<Item1>.Return(item1);
            }
            if (item2 != null)
            {
                if (item2.Clear())
                {
                    Pool<Item2>.Return(item2);
                }
            }
            if (item3 != null)
            {
                Pool<Item3>.Return(item3);
            }
        }
    }

    public static async ValueTask M2()
    {
        Item1 item1 = null;
        try
        {
            item1 = Pool<Item1>.Get();
            var item2 = new Item2();
            await Task.Yield();
            var item3 = new Item3();
            Console.WriteLine($"{item1}, {item2}, {item3}");
        }
        finally
        {
            if (item1 != null)
            {
                item1.GetAndDelete();
                Pool<Item1>.Return(item1);
            }
        }
    }
}
```

As shown, in the `M1` method, there's a difference in how `item1` and `item2` handle the reset call. This is because `Item2`'s reset method returns a `bool`, and Pooling uses that return value to determine whether the reset succeeded. For `void` or other return types, Pooling assumes the reset succeeded if the method returns without error.

### Zero-Intrusion Pooling

Seeing the title might be a bit confusing—after just discussing non-intrusive practices, why is there now a "zero-intrusive" practice, and what’s the difference?

In the non-intrusive pooling operation introduced earlier, we don't need to change any C# code to implement the specified type pooling. However, we still need to add the Pooling.Fody NuGet dependency and modify the `FodyWeavers.xml` for configuration. This still requires some manual steps from the developer. So, how can we eliminate any manual operation for developers? The answer is simple: move these steps to the CI process or release process. That's right—zero-intrusive refers to the developer's perspective. It doesn’t mean nothing needs to be done, but rather that adding NuGet references and configuring `FodyWeavers.xml` is deferred to the CI/release process.

#### What are the advantages?

Optimization techniques like object pooling are often not just needed by a single project—they are typically general optimizations. In this case, rather than modifying one project at a time, it is faster to configure them uniformly in the CI or release process. Furthermore, when dealing with legacy projects, no one might want to change any code, even if it’s just modifying the project file or the `FodyWeavers.xml` configuration file. In such cases, the CI/release process can be modified to handle these changes. Of course, modifying the unified CI/release process might have a broader impact, but this is just to present a zero-intrusive idea. The actual implementation should be carefully considered based on the situation.

#### How to implement it?

The most straightforward way is to add the Pooling.Fody NuGet dependency to the project during the CI build or release process using `dotnet add package Pooling.Fody`, and then copy the pre-configured `FodyWeavers.xml` to the project directory. However, if the project also references other Fody plugins, directly overwriting the existing `FodyWeavers.xml` might invalidate those plugins. Of course, you could make it more complex by controlling the content of `FodyWeavers.xml` through scripts. Here, I recommend a .NET CLI tool called [Cli4Fody](https://github.com/inversionhourglass/Cli4Fody), which can complete both NuGet dependency additions and `FodyWeavers.xml` configurations in one step.

```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Pooling>
    <Items>
      <Item pattern="A.B.C.Item1.GetAndDelete" />
      <Item pattern="Item2.Clear" inspect="execution(* Test.M1(..))" />
      <Item stateless="*..Item3" not-inspect="method(* Test.M2())" />
	</Items>
  </Pooling>
</Weavers>
```

The corresponding command for the FodyWeavers.xml mentioned above using Cli4Fody is:

```shell
fody-cli MySolution.sln \
        --addin Pooling -pv 0.1.0 \
            -n Items:Item -a "pattern=A.B.C.Item1.GetAndDelete" \
            -n Items:Item -a "pattern=Item2.Clear" -a "inspect=execution(* Test.M1(..))" \
            -n Items:Item -a "stateless=*..Item3" -a "not-inspect=method(* Test.M2())"
```

The advantage of Cli4Fody is that it allows NuGet references and FodyWeavers.xml configuration to be completed simultaneously without modifying or deleting configurations of other Fody plugins in the FodyWeavers.xml file. For more information about Cli4Fody configurations, refer to: [https://github.com/inversionhourglass/Cli4Fody](https://github.com/inversionhourglass/Cli4Fody).

#### Rougamo Zero-Intrusion Optimization Example

[Rougamo](https://github.com/inversionhourglass/Rougamo), a static code weaving AOP component, introduced struct support in version 2.2.0, which can optimize GC through structs. However, using structs is not as convenient as classes since they cannot inherit from a base class and can only implement interfaces. As a result, many default implementations in `MoAttribute` need to be repeatedly implemented when defining structs. Now, you can use Pooling to optimize Rougamo's GC through object pooling. In this example, Docker will be used to demonstrate how to achieve zero-intrusion pooling optimization with Cli4Fody during the Docker build process.

Directory structure:
```
.
├── Lib
│   └── Lib.csproj                       # 依赖Rougamo.Fody
│   └── TestAttribute.cs                 # 继承MoAttribute
└── RougamoPoolingConsoleApp
    └── BenchmarkTest.cs
    └── Dockerfile
    └── RougamoPoolingConsoleApp.csproj  # 引用Lib.csproj，没有任何Fody插件依赖
    └── Program.cs
```

The test project defines two empty test methods, `M` and `N`, in `BenchmarkTest.cs`, and both methods are decorated with `TestAttribute`. This test will use Cli4Fody in the Docker build step to add the Pooling.Fody dependency to the project and configure `TestAttribute` as a pooled type. Additionally, it will set the pooling restriction to only occur in the `TestAttribute.M` method. Finally, a benchmark will be conducted to compare the GC performance between `M` and `N`.

```csharp
// TestAttribute
public class TestAttribute : MoAttribute
{
    // To make the GC effects more apparent, each `TestAttribute` will hold a byte array of length 1024.
    private readonly byte[] _occupy = new byte[1024];
}

// BenchmarkTest
public class BenchmarkTest
{
    [Benchmark]
    [Test]
    public void M() { }

    [Benchmark]
    [Test]
    public void N() { }
}

// Program
var config = ManualConfig.Create(DefaultConfig.Instance)
    .AddDiagnoser(MemoryDiagnoser.Default);
var _ = BenchmarkRunner.Run<BenchmarkTest>(config);
```

**Dockerfile**
```docker
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /src
COPY . .

ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install -g Cli4Fody
RUN fody-cli DockerSample.sln --addin Rougamo -pv 4.0.4 --addin Pooling -pv 0.1.0 -n Items:Item -a "stateless=Rougamo.IMo+" -a "inspect=method(* RougamoPoolingConsoleApp.BenchmarkTest.M(..))"

RUN dotnet restore

RUN dotnet publish "./RougamoPoolingConsoleApp/RougamoPoolingConsoleApp.csproj" -c Release -o /src/bin/publish

WORKDIR /src/bin/publish
ENTRYPOINT ["dotnet", "RougamoPoolingConsoleApp.dll"]
```

Through Cli4Fody, the `TestAttribute` woven into `BenchmarkTest.M` has undergone pooling operations, while the `TestAttribute` woven into `BenchmarkTest.N` has not. The final Benchmark results are as follows:
```
| Method | Mean     | Error   | StdDev   | Gen0   | Gen1   | Allocated |
|------- |---------:|--------:|---------:|-------:|-------:|----------:|
| M      | 188.7 ns | 3.81 ns |  6.67 ns | 0.0210 |      - |     264 B |
| N      | 195.5 ns | 4.09 ns | 11.74 ns | 0.1090 | 0.0002 |    1368 B |
```

The complete example code is stored at: https://github.com/inversionhourglass/Pooling/tree/master/samples/DockerSample

In this example, the object pooling optimization for Rougamo is achieved through the use of Cli4Fody in the Docker build steps, making the entire process completely seamless and non-intrusive for developers. If you plan to optimize Rougamo using this method, it's important to note that the aspect type `TestAttribute` in the current example is stateless. Therefore, you need to confirm with the developers that all defined aspect types are stateless. For stateful aspect types, you must define a reset method and use the `pattern` attribute instead of the `stateless` attribute when defining the Item node.

There's one more point you might not have noticed: only the Lib project references `Rougamo.Fody`, and the `RougamoPoolingConsoleApp` project does not reference `Rougamo.Fody`. By default, the `TestAttribute` applied to `BenchmarkTest` should not be effective. However, in this example, it does work. This is because when using Cli4Fody, related parameters for Rougamo were also specified, which causes Cli4Fody to add a reference to `Rougamo.Fody` for `RougamoPoolingConsoleApp`. Thus, Cli4Fody can also help avoid missing direct dependencies for Fody plugins across projects. For more information on Cli4Fody, please refer to https://github.com/inversionhourglass/Cli4Fody.

## Configuration

In [Non-Intrusive Pooling](#non-intrusive-pooling), we introduced the `Items` node configuration. Besides `Items`, Pooling provides several other configuration options. Below is a complete example configuration:

```xml
<Pooling enabled="true" composite-accessibility="false">
  <Inspects>
    <Inspect>any_aspectn_pattern</Inspect>
    <Inspect>any_aspectn_pattern</Inspect>
  </Inspects>
  <NotInspects>
    <NotInspect>any_aspectn_pattern</NotInspect>
    <NotInspect>any_aspectn_pattern</NotInspect>
  </NotInspects>
  <Items>
    <Item pattern="method_name_pattern" stateless="type_pattern" inspect="any_aspectn_pattern" not-inspect="any_aspectn_pattern" />
    <Item pattern="method_name_pattern" stateless="type_pattern" inspect="any_aspectn_pattern" not-inspect="any_aspectn_pattern" />
  </Items>
</Pooling>
```

| Node Path                          | Attribute Name            | Purpose                                                                                     |
|:-----------------------------------|:-------------------------:|:--------------------------------------------------------------------------------------------|
| **/Pooling**                       | enabled                   | Whether Pooling is enabled                                                                  |
| **/Pooling**                       | composite-accessibility   | Whether AspectN uses class+method composite accessibility to match. By default, only method-level accessibility is considered. If a class is `internal` and a method is `public`, the method is treated as `public` by default. Setting this to `true` treats it as `internal`. |
| **/Pooling/Inspects/Inspect**      | [Node Value]              | AspectN expression. Global filter for methods, only methods matching this expression will be inspected for pooled types. This includes `IPoolItem` types. Multiple expressions are combined using `OR`. If omitted, it applies to all methods in the assembly. The final method set is the difference between this and **/Pooling/NotInspects**. |
| **/Pooling/NotInspects/NotInspect**| [Node Value]              | AspectN expression. Global filter for methods, any methods matching this expression will not use pooling. Multiple expressions are combined using `OR`. If omitted, no methods are excluded. The final method set is the difference between **/Pooling/Inspects** and this. |
| **/Pooling/Items/Item**            | pattern                   | AspectN type+method expression. Matches types for pooling and methods for reset operations. Reset methods must be parameterless. If the return type is `bool`, Pooling treats it as an indication of success. Use either this or `stateless`, not both. |
| **/Pooling/Items/Item**            | stateless                 | AspectN type expression. Matches types for pooling as stateless objects that do not require reset before being returned to the pool. Use either this or `pattern`, not both. |
| **/Pooling/Items/Item**            | inspect                   | AspectN expression. Specifies the methods in which pooled types can be used. The final method set is the difference between this and `not-inspect`. If omitted, it applies to all methods in the assembly. |
| **/Pooling/Items/Item**            | not-inspect               | AspectN expression. Specifies methods where pooling will not occur. The final method set is the difference between `inspect` and this. If omitted, no methods are excluded. |

As seen, many configuration options rely on AspectN expressions. For more details on how to use these expressions, see: https://github.com/inversionhourglass/Shared.Cecil.AspectN/blob/master/README.md

**Please note that using AspectN is like using pointers in memory. Matching unexpected types as pooled types can lead to concurrency issues where the same object instance is used simultaneously. Be cautious when using AspectN prefer precise matches over vague ones.**

## Object Pool Configuration

### Maximum Object Retention in Object Pools

By default, each object pool can retain up to a number of objects equal to twice the number of logical processors (`Environment.ProcessorCount * 2`). There are two ways to modify this setting:

1. **Specify in Code**

   You can set the maximum retained objects for all types using `Pool.GenericMaximumRetained`, or for a specific type using `Pool<T>.MaximumRetained`. The latter takes precedence over the former.

2. **Specify Using Environment Variables**

   You can set the maximum retained objects by specifying an environment variable when the application starts. The variable `NET_POOLING_MAX_RETAIN` controls the maximum retained objects for all types, while `NET_POOLING_MAX_RETAIN_{PoolItemFullName}` allows you to configure the retention for specific types. `{PoolItemFullName}` refers to the fully qualified name of the pool type (namespace.classname), where periods (`.`) in the name should be replaced with underscores (`_`). For example, use `NET_POOLING_MAX_RETAIN_System_Text_StringBuilder`. Environment variables take precedence over code settings, making them a more flexible option for controlling pool behavior.

### Custom Object Pool

Although there is an official object pool library (`Microsoft.Extensions.ObjectPool`), Pooling uses its own implementation. This decision is because Pooling is a compile-time component, and method calls are woven directly into IL code. If Pooling were to rely on a third-party library, any signature changes in future updates could result in `MethodNotFoundException` errors at runtime. To minimize such risks, Pooling avoids third-party dependencies.

Despite concerns about performance, the custom pool used in Pooling is derived from `Microsoft.Extensions.ObjectPool`, but it has been streamlined to remove elements like `ObjectPoolProvider` and `PooledObjectPolicy`, keeping the implementation minimal and efficient. However, you can still customize the object pool by implementing the `IPool` interface for general pools or the `IPool<T>` interface for specific type pools. Here is an example of how you can replace the default pool with `Microsoft.Extensions.ObjectPool`:

```csharp
// General object pool
public class MicrosoftPool : IPool
{
    private static readonly ConcurrentDictionary<Type, object> _Pools = [];

    public T Get<T>() where T : class, new()
    {
        return GetPool<T>().Get();
    }

    public void Return<T>(T value) where T : class, new()
    {
        GetPool<T>().Return(value);
    }

    private ObjectPool<T> GetPool<T>() where T : class, new()
    {
        return (ObjectPool<T>)_Pools.GetOrAdd(typeof(T), t =>
        {
            var provider = new DefaultObjectPoolProvider();
            var policy = new DefaultPooledObjectPolicy<T>();
            return provider.Create(policy);
        });
    }
}

// Specific type object pool
public class SpecificalMicrosoftPool<T> : IPool<T> where T : class, new()
{
    private readonly ObjectPool<T> _pool;

    public SpecificalMicrosoftPool()
    {
        var provider = new DefaultObjectPoolProvider();
        var policy = new DefaultPooledObjectPolicy<T>();
        _pool = provider.Create(policy);
    }

    public T Get()
    {
        return _pool.Get();
    }

    public void Return(T value)
    {
        _pool.Return(value);
    }
}

// It is best to replace the pool implementation in the Main entry before the pool is used to prevent further replacement attempts

// Replace general object pool implementation
Pool.Set(new MicrosoftPool());
// Replace specific type object pool
Pool<Xyz>.Set(new SpecificalMicrosoftPool<Xyz>());
```

## Not Only Object Pools

Although Pooling is primarily intended to simplify object pool usage and enable non-intrusive project optimizations, it can do much more due to its flexible design and custom pool functionality. Essentially, Pooling acts like a probe that intercepts `new` operations with no-argument constructors, allowing you to perform various tasks. Here are some examples of what you can achieve beyond simple object pooling.

### Singleton

```csharp
// Define singleton object pool
public class SingletonPool<T> : IPool<T> where T : class, new()
{
    private readonly T _value = new();

    public T Get() => _value;

    public void Return(T value) { }
}

// Replace object pool implementation
Pool<ConcurrentDictionary<Type, object>>.Set(new SingletonPool<ConcurrentDictionary<Type, object>>());

// In your configuration, set ConcurrentDictionary<Type, object> as a pooled type
// <Item stateless="System.Collections.Concurrent.ConcurrentDictionary&lt;System.Type, object&gt;" />
```

With this configuration, all instances of `ConcurrentDictionary<Type, object>>` will share a single object instance.

### Semaphore Control

```csharp
// Define semaphore object pool
public class SemaphorePool<T> : IPool<T> where T : class, new()
{
    private readonly Semaphore _semaphore = new(3, 3);
    private readonly DefaultPool<T> _pool = new();

    public T Get()
    {
        if (!_semaphore.WaitOne(100)) return null;
        return _pool.Get();
    }

    public void Return(T value)
    {
        _pool.Return(value);
        _semaphore.Release();
    }
}

// Replace object pool implementation
Pool<Connection>.Set(new SemaphorePool<Connection>());

// In your configuration, set Connection as a pooled type
// <Item stateless="X.Y.Z.Connection" />
```

This example uses a semaphore to limit the number of `Connection` objects, making it useful for rate-limiting scenarios.

### Thread-local Singleton

```csharp
// Define thread-local singleton object pool
public class ThreadLocalPool<T> : IPool<T> where T : class, new()
{
    private readonly ThreadLocal<T> _random = new(() => new());

    public T Get() => _random.Value!;

    public void Return(T value) { }
}

// Replace object pool implementation
Pool<Random>.Set(new ThreadLocalPool<Random>());

// In your configuration, set Random as a pooled type
// <Item stateless="System.Random" />
```

This allows you to implement a thread-local singleton, useful when you want to reduce GC pressure without sacrificing thread safety.

### Additional Initialization

```csharp
// Define property injection object pool
public class ServiceSetupPool : IPool<Service1>
{
    public Service1 Get()
    {
        var service1 = new Service1();
        var service2 = PinnedScope.ScopedServices?.GetService<Service2>();
        service1.Service2 = service2;
        return service1;
    }

    public void Return(Service1 value) { }
}

// Define pooled types
public class Service2 { }

[PoolingExclusive(Types = [typeof(ServiceSetupPool)])]
public class Service1 : IPoolItem
{
    public Service2? Service2 { get; set; }

    public bool TryReset() => true;
}

// Replace object pool implementation
Pool<Service1>.Set(new ServiceSetupPool());
```

In this example, Pooling is combined with [DependencyInjection.StaticAccessor](https://github.com/inversionhourglass/DependencyInjection.StaticAccessor) to achieve property injection, which can also be used for other initialization operations.

### Unleashing Your Creativity

The examples provided may not be immediately practical, but their main purpose is to inspire you to think outside the box. By understanding that Pooling replaces temporary `new` operations with object pool operations and the extensibility of custom pools, you may find creative uses for Pooling in future scenarios where it can help you implement solutions quickly with minimal code changes.

## Precautions

1. **Avoid performing reuse-time initialization in the constructor of pooled types**

    > Objects retrieved from the pool may be reused and won't execute the constructor again. If you need some initialization to run every time an object is reused, you should separate that operation into a method and call it after the object is created, instead of placing it in the constructor.
    ```csharp
    // Before modification - Pooled object definition
    public class Connection : IPoolItem
    {
        private readonly Socket _socket;

        public Connection()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Initialization like Connect should not be here. It should be moved to a separate method.
            _socket.Connect("127.0.0.1", 8888);
        }

        public void Write(string message)
        {
            // ...
        }

        public bool TryReset()
        {
            _socket.Disconnect(true);
            return true;
        }
    }
    // Before modification - Pooled object usage
    var connection = new Connection();
    connection.Write("message");

    // After modification - Pooled object definition
    public class Connection : IPoolItem
    {
        private readonly Socket _socket;

        public Connection()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect()
        {
            _socket.Connect("127.0.0.1", 8888);
        }

        public void Write(string message)
        {
            // ...
        }

        public bool TryReset()
        {
            _socket.Disconnect(true);
            return true;
        }
    }
    // After modification - Pooled object usage
    var connection = new Connection();
    connection.Connect();
    connection.Write("message");
    ```

2. **Only no-argument constructor `new` operations are supported for pooling**

    > Since reused objects do not execute the constructor again, constructor arguments are meaningless for pooled objects. If initialization needs to be done with parameters, create an initialization method or use properties.
    > 
    > During compilation, Pooling will check whether the `new` operation calls a no-argument constructor. If it calls a constructor with arguments, Pooling will not replace this `new` operation with a pool operation.

3. **Be careful not to persist pooled type instances**

    > Pooling's object pool operations are method-level, meaning pooled objects are created and released within the same method. Persisting pooled objects in fields poses a risk of concurrent access. If the lifecycle of a pooled object spans multiple methods, you should manually create and manage the pool.
    > 
    > During compilation, Pooling will perform a simple persistence check. If an object is found to be persisted, pooling will not be applied. However, this check only applies to simple persistence cases and cannot detect complex scenarios. Therefore, care should be taken to avoid persisting pooled objects.

4. **All assemblies requiring pool operation replacements must reference `Pooling.Fody`**

    > Pooling works by examining the MSIL of all methods (or a selected subset via configuration) during compilation, identifying all `newobj` operations, and replacing them with pool operations. This process is triggered by a MSBuild task added by Fody, which requires direct reference to Fody in the current assembly. By referencing `Pooling.Fody`, this task can be added to any project.

5. **Consideration when using multiple Fody plugins**

    > When a Fody plugin is referenced in a project, a `FodyWeavers.xml` file is automatically generated during compilation. If another Fody plugin is referenced later, it will not automatically add itself to the existing `FodyWeavers.xml` file. Manual configuration is required. Additionally, the order of plugins in `FodyWeavers.xml` matters, as it dictates the order in which the plugins are executed. Some Fody plugins may have overlapping functionalities, and different execution orders may lead to different results.
