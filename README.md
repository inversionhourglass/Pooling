# Pooling

中文 | [English](https://github.com/inversionhourglass/Pooling/blob/master/README_en.md)

Pooling是一个编译时对象池组件，通过在编译时将`new`操作替换为对象池操作，简化编码过程，无需开发人员手动编写对象池操作代码。同时提供了完全无侵入式的解决方案，可用作临时性能优化的解决方案和老久项目性能优化的解决方案等。

## 快速开始

引用Pooling.Fody
> dotnet add package Pooling.Fody

确保`FodyWeavers.xml`文件中已配置Pooling，如果当前项目没有`FodyWeavers.xml`文件，可以直接编译项目，会自动生成`FodyWeavers.xml`文件：
```xml
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Pooling /> <!--确保存在Pooling节点-->
</Weavers>
```

```csharp
// 1. 需要池化的类型实现IPoolItem接口
public class TestItem : IPoolItem
{
    public int Value { get; set; }

    // 当对象返回对象池化时通过该方法进行重置实例状态
    public bool TryReset()
    {
        return true;
    }
}

// 2. 在任何地方使用new关键字创建该类型的对象
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

// 编译后代码
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

正如[快速开始](#快速开始)中的代码所示，实现了`IPoolItem`接口的类型便是一个池化类型，在编译时Pooling会将其new操作替换为对象池操作，并在finally块中将池化对象实例返还到对象池中。`IPoolItem`仅有一个`TryReset`方法，该方法用于在对象返回对象池时进行状态重置，该方法返回false时表示状态重置失败，此时该对象将会被丢弃。

### PoolingExclusiveAttribute

默认情况下，实现`IPoolItem`的池化类型会在当前程序集的所有方法中进行池化操作，但有时候我们可能希望该池化类型在部分类型中不进行池化操作，比如我们可能会创建一些池化类型的管理类型或者Builder类型，此时在池化类型上应用`PoolingExclusiveAttribute`便可指定该池化类型不在某些类型/方法中进行池化操作。

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
        // 由于通过PoolingExclusive的Types属性排除了TestItemBuilder，所以这里不会替换为对象池操作
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
        // 由于通过PoolingExclusive的Pattern属性排除了TestItemManager下的所有方法，所以这里不会替换为对象池操作
        var item = _cacheItem ?? new TestItem();
        // ...
    }
}
```

如上代码所示，`PoolingExclusiveAttribute`有两个属性`Types`和`Pattern`。`Types`为`Type`类型数组，当前池化类型不会在数组中的类型的方法中进行池化操作；`Pattern`为`string`类型AspectN表达式，可以细致的匹配到具体的方法（AspectN表达式格式详见：https://github.com/inversionhourglass/Shared.Cecil.AspectN/blob/master/README.md ），当前池化类型不会在被匹配到的方法中进行池化操作。两个属性可以使用其中一个，也可以同时使用，同时使用时将排除两个属性匹配到的所有类型/方法。

## NonPooledAttribute

前面介绍了可以通过`PoolingExclusiveAttribute`指定当前池化对象在某些类型/方法中不进行池化操作，但由于`PoolingExclusiveAttribute`需要直接应用到池化类型上，所以如果你使用了第三方类库中的池化类型，此时你无法直接将`PoolingExclusiveAttribute`应用到该池化类型上。针对此类情况，可以使用`NonPooledAttribute`表明当前方法不进行池化操作。

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
        // 由于方法应用了NonPooledAttribute，以下三个new操作都不会替换为对象池操作
        var item1 = new TestItem1();
        var item2 = new TestItem2();
        var item3 = new TestItem3();
    }
}
```

有的时候你可能并不是希望方法里所有的池化类型都不进行池化操作，此时可以通过`NonPooledAttribute`的两个属性`Types`和`Pattern`指定不可进行池化操作的池化类型。`Types`为`Type`类型数组，数组中的所有类型在当前方法中均不可进行池化操作；`Pattern`为`string`类型AspectN类型表达式，所有匹配的类型在当前方法中均不可进行池化操作。

```csharp
public class Test
{
    [NonPooled(Types = [typeof(TestItem1)], Pattern = "*..TestItem3")]
    public void M()
    {
        // TestItem1通过Types不允许进行池化操作，TestItem3通过Pattern不允许进行池化操作，仅TestItem2可进行池化操作
        var item1 = new TestItem1();
        var item2 = new TestItem2();
        var item3 = new TestItem3();
    }
}
```

AspectN类型表达式灵活多变，支持逻辑非操作符`!`，所以可以很方便的使用AspectN类型表达式仅允许某一个类型，比如上面的示例可以简单改为`[NonPooled(Pattern = "!TestItem2")]`，更多AspectN表达式说明，详见：https://github.com/inversionhourglass/Shared.Cecil.AspectN/blob/master/README.md 。

`NonPooledAttribute`不仅可以应用于方法层级，还可以应用于类型和程序集。应用于类等同于应用到类的所有方法上（包括属性和构造方法），应用于程序集等同于应用到当前程序集的所有方法上（包括属性和构造方法），另外如果在应用到程序集时没有指定`Types`和`Pattern`两个属性，那么就等同于当前程序集禁用Pooling。

## 无侵入式池化操作

前面介绍的`IPoolItem`需要手动更改代码完成接入，适用于新项目接入及小型项目改造，对于庞大的老久项目，这样的改动可能牵扯甚广，考虑到可能带来的风险，可能就懒得改了。Pooling提供了无侵入式的接入方式，不需要实现`IPoolItem`接口，通过配置即可指定池化类型。

假设目前有如下代码：

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

项目在引用`Pooling.Fody`后，编译项目时项目文件夹下会生成一个`FodyWeavers.xml`文件，我们按下面的示例修改`Pooling`节点：

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

上面的配置中，每一个`Item`节点匹配一个池化类型，上面的配置中展示了全部的四个属性，它们的含义分别是：

- **pattern**: AspectN类型+方法表达式。匹配到的类型为池化类型，匹配到的方法为状态重置方法（等同于IPoolItem的TryReset方法）。需要注意的是，重置方法必须是无参的。
- **stateless**: AspectN类型表达式。匹配到的类型为池化类型，该类型为无状态类型，不需要重置操作即可回到对象池中。
- **inspect**: AspectN表达式。`pattern`和`stateless`匹配到的池化类型，只有在该表达式匹配到的方法中才会进行池化操作。当该配置缺省时表示匹配当前程序集的所有方法。
- **not-inspect**: AspectN表达式。`pattern`和`stateless`匹配到的池化类型不会在该表达式匹配到的方法中进行池化操作。当该配置缺省时表示不排除任何方法。最终池化类型能够进行池化操作的方法集合为`inspect`集合与`not-inspect`集合的差集。

那么通过上面的配置，`Test`在编译后的代码为：

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

细心的你可能注意到在`M1`方法中，`item1`和`item2`在重置方法的调用上有所区别，这是因为`Item2`的重置方法的返回值类型为`bool`，Poolinng会将其结果作为是否重置成功的依据，对于`void`或其他类型的返回值，Pooling将在方法成功返回后默认其重置成功。

### 零侵入式池化操作

看到标题是不是有点懵，刚介绍完无侵入式，怎么又来个零侵入式，它们有什么区别？

在上面介绍的无侵入式池化操作中，我们不需要改动任何C#代码即可完成指定类型池化操作，但我们仍需要添加Pooling.Fody的NuGet依赖，并且需要修改FodyWeavers.xml进行配置，这仍然需要开发人员手动操作完成。那如何让开发人员完全不需要任何操作呢？答案也很简单，就是将这一步放到CI流程或发布流程中完成。是的，零侵入是针对开发人员的，并不是真的什么都不需要做，而是将引用NuGet和配置FodyWeavers.xml的步骤延后到CI/发布流程中了。

#### 优势是什么

类似于对象池这类型的优化往往不是仅仅某一个项目需要优化，这种优化可能是普遍性的，那么此时相比一个项目一个项目的修改，统一的在CI流程/发布流程中配置是更为快速的选择。另外在面对一些古董项目时，可能没有人愿意去更改任何代码，即使只是项目文件和FodyWeavers.xml配置文件，此时也可以通过修改CI/发布流程来完成。当然修改统一的CI/发布流程的影响面可能更广，这里只是提供一种零侵入式的思路，具体情况还需要结合实际情况综合考虑。

#### 如何实现

最直接的方式就是在CI构建流程或发布流程中通过`dotnet add package Pooling.Fody`为项目添加NuGet依赖，然后将预先配置好的FodyWeavers.xml复制到项目目录下。但如果项目还引用了其他Fody插件，直接覆盖原有的FodyWeavers.xml可能导致原有的插件无效。当然，你也可以复杂点通过脚本控制FodyWeavers.xml的内容，这里我推荐一个.NET CLI工具，[Cli4Fody](https://github.com/inversionhourglass/Cli4Fody)可以一步完成NuGet依赖和FodyWeavers.xml配置。

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

上面的FodyWeavers.xml，使用Cli4Fody对应的命令为：

```shell
fody-cli MySolution.sln \
        --addin Pooling -pv 0.1.0 \
            -n Items:Item -a "pattern=A.B.C.Item1.GetAndDelete" \
            -n Items:Item -a "pattern=Item2.Clear" -a "inspect=execution(* Test.M1(..))" \
            -n Items:Item -a "stateless=*..Item3" -a "not-inspect=method(* Test.M2())"
```

Cli4Fody的优势是，NuGet引用和FodyWeavers.xml可以同时完成，并且Cli4Fody并不会修改或删除FodyWeavers.xml中其他Fody插件的配置。更多Cli4Fody相关配置，详见：https://github.com/inversionhourglass/Cli4Fody

#### Rougamo零侵入式优化案例

[肉夹馍（Rougamo）](https://github.com/inversionhourglass/Rougamo)，一款静态代码编织的AOP组件。肉夹馍在2.2.0版本中新增了结构体支持，可以通过结构体优化GC。但结构体的使用没有类方便，不可继承父类只能实现接口，所以很多`MoAttribute`中的默认实现在定义结构体时需要重复实现。现在，你可以使用Pooling通过对象池来优化肉夹馍的GC。在这个示例中将使用Docker演示如何在Docker构建流程中使用Cli4Fody完成零侵入式池化操作：

目录结构：
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

该测试项目在`BenchmarkTest.cs`里面定义了两个空的测试方法`M`和`N`，两个方法都应用了`TestAttribute`。本次测试将在Docker的构建步骤中使用Cli4Fody为项目增加Pooling.Fody依赖并将`TestAttribute`配置为池化类型，同时设置其只能在`TestAttribute.M`方法中进行池化，然后通过Benchmark对比`M`和`N`的GC情况。

```csharp
// TestAttribute
public class TestAttribute : MoAttribute
{
    // 为了让GC效果更明显，每个TestAttribute都将持有长度为1024的字节数组
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

通过Cli4Fody最终`BenchmarkTest.M`中织入的`TestAttribute`进行了池化操作，而`BenchmarkTest.N`中织入的`TestAttribute`没有进行池化操作，最终Benchmark结果如下：
```
| Method | Mean     | Error   | StdDev   | Gen0   | Gen1   | Allocated |
|------- |---------:|--------:|---------:|-------:|-------:|----------:|
| M      | 188.7 ns | 3.81 ns |  6.67 ns | 0.0210 |      - |     264 B |
| N      | 195.5 ns | 4.09 ns | 11.74 ns | 0.1090 | 0.0002 |    1368 B |
```

完整示例代码保存在：https://github.com/inversionhourglass/Pooling/tree/master/samples/DockerSample

在这个示例中，通过在Docker的构建步骤中使用Cli4Fody完成了对Rougamo的对象池优化，整个过程对开发时完全无感零侵入的。如果你准备用这种方法对Rougamo进行对象池优化，需要注意的是当前示例中的切面类型`TestAttribute`是无状态的，所以你需要跟开发确认所有定义的切面类型都是无状态的，对于有状态的切面类型，你需要定义重置方法并在定义Item节点时使用pattern属性而不是stateless属性。

在这个示例中还有一点你可能没有注意，只有Lib项目引用了Rougamo.Fody，RougamoPoolingConsoleApp项目并没有引用Rougamo.Fody，默认情况下应用到`BenchmarkTest`的`TestAttribute`应该是不会生效的，但我这个例子中却生效了。这是因为在使用Cli4Fody时还指定了Rougamo的相关参数，Cli4Fody会为RougamoPoolingConsoleApp添加了Rougamo.Fody引用，所以Cli4Fody也可用于避免遗漏项目队Fody插件的直接依赖，更多Cli4Fody的内容详见：https://github.com/inversionhourglass/Cli4Fody

## 配置项

在[无侵入式池化操作](#无侵入式池化操作)中介绍了`Items`节点配置，除了`Items`配置项Pooling还提供了其他配置项，下面是完整配置示例：

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

| 节点路径                             | 属性名称                | 用途            |
|:------------------------------------|:-----------------------:|:---------------|
| **/Pooling**                        | enabled                 | 是否启用Pooling |
| **/Pooling**                        | composite-accessibility | AspectN是否使用类+方法综合可访问性进行匹配。默认仅按方法可访问性进行匹配，比如类的可访问性为internal，方法的可访问性为public，那么默认情况下该方法的可访问性认定为public，将该配置设置为true后，该方法的可访问性认定为internal |
| **/Pooling/Inspects/Inspect**       | [节点值]                | AspectN表达式。<br>全局筛选器，只有被该表达式匹配的方法才会检查内部是否使用到池化类型并进行池化操作替换。即使是实现了`IPoolItem`的池化类型也会受限于该配置。<br>该节点可配置多条，匹配的方法集合为多条配置的并集。<br>该节点缺省时表示匹配当前程序集所有方法。<br>最终的方法集合是该节点配置匹配的集合与 **/Pooling/NotInspects** 配置匹配的集合的差集。 |
| **/Pooling/NotInspects/NotInspect** | [节点值]                | AspectN表达式。<br>全局筛选器，被该表达式匹配的方法的内部不会进行池化操作替换。即使是实现了`IPoolItem`的池化类型也会受限于该配置。<br>该节点可配置多条，匹配的方法集合为多条配置的并集。<br>该节点缺省时表示不排除任何方法。<br>最终的方法集合是 **/Pooling/Inspects** 配置匹配的集合与该节点配置匹配的集合的差集。 |
| **/Pooling/Items/Item**             | pattern                 | AspectN类型+方法名表达式。<br>匹配的类型会作为池化类型，匹配的方法会作为重置方法。<br>重置方法必须是无参方法，如果方法返回值类型为`bool`，返回值还会被作为是否重置成功的依据。<br>该属性与`stateless`属性仅可二选一。 |
| **/Pooling/Items/Item**             | stateless               | AspectN类型表达式。<br>匹配的类型会作为池化类型，该类型为无状态类型，在回到对象池之前不需要进行重置。<br>该属性与`pattern`仅可二选一。 |
| **/Pooling/Items/Item**             | inspect                 | AspectN表达式。<br>`pattern`和`stateless`匹配到的池化类型，只有在该表达式匹配到的方法中才会进行池化操作。<br>当该配置缺省时表示匹配当前程序集的所有方法。<br>当前池化类型最终能够应用的方法集合为该配置匹配的方法集合与`not-inspect`配置匹配的方法集合的差集。 |
| **/Pooling/Items/Item**             | not-inspect             | AspectN表达式。<br>`pattern`和`stateless`匹配到的池化类型不会在该表达式匹配到的方法中进行池化操作。<br>当该配置缺省时表示不排除任何方法。<br>当前池化类型最终能够应用的方法集合为`inspect`配置匹配的方法集合与该配置匹配的方法集合的差集。 |

可以看到配置中大量使用了AspectN表达式，了解更多AspectN表达式的用法详见：https://github.com/inversionhourglass/Shared.Cecil.AspectN/blob/master/README.md

**另外需要注意的是，程序集中的所有方法就像是内存，而AspectN就像指针，通过指针操作内存时需格外小心。将预期外的类型匹配为池化类型可能会导致同一个对象实例被并发的使用，所以在使用AspectN表达式时尽量使用精确匹配，避免使用模糊匹配。**

## 对象池配置

### 对象池最大对象持有数量

每个池化类型的对象池最大持有对象数量为逻辑处理器数量乘以2`Environment.ProcessorCount * 2`，有两种方式可以修改这一默认设置。
1. **通过代码指定**

    通过`Pool.GenericMaximumRetained`可以设置所有池化类型的对象池最大对象持有数量，通过`Pool<T>.MaximumRetained`可以设置指定池化类型的对象池最大对象持有数量。后者优先级高于前者。

2. **通过环境变量指定**

    在应用启动时指定环境变量可以修改对象池最大持有对象数量，`NET_POOLING_MAX_RETAIN`用于设置所有池化类型的对象池最大对象持有数量，`NET_POOLING_MAX_RETAIN_{PoolItemFullName}`用于设置指定池化类型的对象池最大对象持有数量，其中`{PoolItemFullName}`为池化类型的全名称（命名空间.类名），需要注意的是，需要将全名称中的`.`替换为`_`，比如`NET_POOLING_MAX_RETAIN_System_Text_StringBuilder`。环境变量的优先级高于代码指定，推荐使用环境变量进行控制，更为灵活。

### 自定义对象池

我们知道官方有一个对象池类库`Microsoft.Extensions.ObjectPool`，Pooling没有直接引用这个类库而选择自建对象池，是因为Pooling作为编译时组件，对方法的调用都是通过IL直接织入的，如果引用三方类库，并且三方类库在后续的更新对方法签名有所修改，那么可能会在运行时抛出`MethodNotFoundException`，所以尽量减少三方依赖是编译时组件最好的选择。

有的朋友可能会担心自建对象池的性能问题，可以放心的是Pooling对象池的实现是从`Microsoft.Extensions.ObjectPool`拷贝而来，同时精简了`ObjectPoolProvider`, `PooledObjectPolicy`等元素，保持最精简的默认对象池实现。同时，Pooling支持自定义对象池，实现`IPool`接口定义通用对象池，实现`IPool<T>`接口定义特定池化类型的对象池。下面简单演示如何通过自定义对象池将对象池实现换为`Microsoft.Extensions.ObjectPool`：

```csharp
// 通用对象池
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

// 特定池化类型对象池
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

// 替换操作最好在Main入口直接完成，一旦对象池被使用就不再运行进行替换操作

// 替换通用对象池实现
Pool.Set(new MicrosoftPool());
// 替换特定类型对象池
Pool<Xyz>.Set(new SpecificalMicrosoftPool<Xyz>());
```

## 不仅仅用作对象池

虽然Pooling的意图是简化对象池操作和无侵入式的项目改造优化，但得益于Pooling的实现方式以及提供的自定义对象池功能，你可以使用Pooling完成的事情不仅仅是对象池，Pooling的实现相当于在所有无参构造方法调用的地方埋入了一个探针，你可以在这里做任何事情，下面简单举几个例子。

### 单例

```csharp
// 定义单例对象池
public class SingletonPool<T> : IPool<T> where T : class, new()
{
    private readonly T _value = new();

    public T Get() => _value;

    public void Return(T value) { }
}

// 替换对象池实现
Pool<ConcurrentDictionary<Type, object>>.Set(new SingletonPool<ConcurrentDictionary<Type, object>>());

// 通过配置，将ConcurrentDictionary<Type, object>设置为池化类型
// <Item stateless="System.Collections.Concurrent.ConcurrentDictionary&lt;System.Type, object&gt;" />
```

通过上面的改动，你成功的让所有的`ConcurrentDictionary<Type, object>>`共享一个实例。

### 控制信号量

```csharp
// 定义信号量对象池
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

// 替换对象池实现
Pool<Connection>.Set(new SemaphorePool<Connection>());

// 通过配置，将Connection设置为池化类型
// <Item stateless="X.Y.Z.Connection" />
```

在这个例子中使用信号量对象池控制`Connection`的数量，对于一些限流场景非常适用。

### 线程单例

```csharp
// 定义现成单例对象池
public class ThreadLocalPool<T> : IPool<T> where T : class, new()
{
    private readonly ThreadLocal<T> _random = new(() => new());

    public T Get() => _random.Value!;

    public void Return(T value) { }
}

// 替换对象池实现
Pool<Random>.Set(new ThreadLocalPool<Random>());

// 通过配置，将Connection设置为池化类型
// <Item stateless="System.Random" />
```

当你想通过单例来减少GC压力但对象又不是线程安全的，此时便可以`ThreadLocal`实现线程内单例。

### 额外的初始化

```csharp
// 定义现属性注入对象池
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

// 定义池化类型
public class Service2 { }

[PoolingExclusive(Types = [typeof(ServiceSetupPool)])]
public class Service1 : IPoolItem
{
    public Service2? Service2 { get; set; }

    public bool TryReset() => true;
}

// 替换对象池实现
Pool<Service1>.Set(new ServiceSetupPool());
```

在这个例子中使用Pooling结合[DependencyInjection.StaticAccessor](https://github.com/inversionhourglass/DependencyInjection.StaticAccessor)完成属性注入，使用相同方式可以完成其他初始化操作。

### 发挥想象力

前面的这些例子可能不一定实用，这些例子的主要目的是启发大家开拓思路，理解Pooling的基本实现原理是将临时变量的new操作替换为对象池操作，理解自定义对象池的可扩展性。也许你现在用不上Pooling，但未来的某个需求场景下，你可能可以用Pooling快速实现而不需要大量改动代码。

## 注意事项

1. **不要在池化类型的构造方法中执行复用时的初始化操作**

    > 从对象池中获取的对象可能是复用的对象，被复用的对象是不会再次执行构造方法的，所以如果你有一些初始化操作希望每次复用时都执行，那么你应该将该操作独立到一个方法中并在new操作后调用而不应该放在构造方法中
    ```csharp
    // 修改前池化对象定义
    public class Connection : IPoolItem
    {
        private readonly Socket _socket;

        public Connection()
        {
            _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // 不应该在这里Connect，应该将Connect操作单独独立为一个方法，然后再new操作后调用
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
    // 修改前池化对象使用
    var connection = new Connection();
    connection.Write("message");

    // 修改后池化对象定义
    public class Connection : IPoolItem
    {
        private readonly Socket _socket;

        public Connection()
        {
            _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
    // 修改后池化对象使用
    var connection = new Connection();
    connection.Connect();
    connection.Write("message");
    ```

2. **仅支持将无参构造方法的new操作替换为对象池操作**

    > 由于复用的对象无法再次执行构造方法，所以构造参数对于池化对象毫无意义。如果希望通过构造参数完成一些初始化操作，可以将新建一个初始化方法接收这些参数并完成初始化，或通过属性接收这些参数。
    > 
    > Pooling在编译时会检查new操作是否调用了无参构造方法，如果调用了有参构造方法，将不会将本次new操作替换为对象池操作。

3. **注意不要将池化类型实例进行持久化保存**

    > Pooling的对象池操作是方法级别的，也就是池化对象在当前方法中创建也在当前方法结束时释放，不可将池化对象持久化到字段之中，否则会存在并发使用的风险。如果池化对象的声明周期跨越了多个方法，那么你应该手动创建对象池并手动管理该对象。
    > 
    > Pooling在编译时会进行简单的持久化排查，对于排查出来的池化对象将不进行池化操作。但需要注意的是，这种排查仅可排查一些简单的持久化操作，无法排查出复杂情况下的持久化操作，比如你在当前方法中调用另一个方法传入了池化对象实例，然后在被调用方法中进行持久化操作。所以根本上还是需要你自己注意，避免将池化对象持久化保存。

4. **需要编译时进行对象池操作替换的程序集都需要引用Pooling.Fody**

    > Pooling的原理是在编译时检查所有方法（也可以通过配置选择部分方法）的MSIL，排查所有newobj操作完成对象池替换操作，触发该操作是通过Fody添加了一个MSBuild任务完成的，而只有当前程序集直接引用了Fody才能够完成添加MSBuild任务这一操作。Pooling.Fody通过一些配置使得直接引用Pooling.Fody也可完成添加MSBuild任务的操作。

5. **多个Fody插件同时使用时的注意事项**

    > 当项目引用了一个Fody插件时，在编译时会自动生成一个`FodyWeavers.xml`文件，如果在`FodyWeavers.xml`文件已存在的情况下再引用一个其他Fody插件，此时再编译，新的插件将不会追加到`FodyWeavers.xml`文件中，需要手动配置。同时在引用多个Fody插件时需要注意他们在`FodyWeavers.xml`中的顺序，`FodyWeavers.xml`顺序对应着插件执行顺序，部分Fody插件可能存在功能交叉，不同的顺序可能产生不同的效果。
