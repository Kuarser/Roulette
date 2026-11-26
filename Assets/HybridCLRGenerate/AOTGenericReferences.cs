using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"Newtonsoft.Json.dll",
		"System.Core.dll",
		"System.Memory.dll",
		"System.Runtime.CompilerServices.Unsafe.dll",
		"System.dll",
		"UnityEngine.AndroidJNIModule.dll",
		"UnityEngine.AssetBundleModule.dll",
		"UnityEngine.CoreModule.dll",
		"UnityEngine.JSONSerializeModule.dll",
		"UnityEngine.UI.dll",
		"mscorlib.dll",
		"zxing.unity.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// System.Action<CryPrinter.Pixel>
	// System.Action<Loom.DelayedQueueItem>
	// System.Action<Loom.NoDelayedQueueItem>
	// System.Action<RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Action<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Action<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Action<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Action<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Action<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Action<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Action<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Action<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Action<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Action<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Action<System.Guid>
	// System.Action<UnityEngine.EventSystems.RaycastResult>
	// System.Action<UnityEngine.Rect,object,UnityEngine.Rect,UnityEngine.Color>
	// System.Action<UnityEngine.Vector2>
	// System.Action<byte,object,byte>
	// System.Action<byte,uint,uint,uint>
	// System.Action<byte,ulong>
	// System.Action<byte>
	// System.Action<float,float>
	// System.Action<float>
	// System.Action<int,byte,byte>
	// System.Action<int,byte>
	// System.Action<int,int,int>
	// System.Action<int,int>
	// System.Action<int,long>
	// System.Action<int,object,int>
	// System.Action<int,object,object,object>
	// System.Action<int,object>
	// System.Action<int,uint>
	// System.Action<int>
	// System.Action<long,byte,int,byte>
	// System.Action<long,byte>
	// System.Action<long>
	// System.Action<object,byte,long,short,short,object>
	// System.Action<object,byte>
	// System.Action<object,int,int>
	// System.Action<object,int,uint,int>
	// System.Action<object,int,uint>
	// System.Action<object,int>
	// System.Action<object,long>
	// System.Action<object,object,byte>
	// System.Action<object,object,int>
	// System.Action<object,object,object,object>
	// System.Action<object,object,object,uint>
	// System.Action<object,object,object>
	// System.Action<object,object,uint,object>
	// System.Action<object,object,uint,uint>
	// System.Action<object,object,uint>
	// System.Action<object,object>
	// System.Action<object,short,short>
	// System.Action<object,uint,object,int,object>
	// System.Action<object,uint,object,uint>
	// System.Action<object,uint,object>
	// System.Action<object,uint,uint>
	// System.Action<object,uint>
	// System.Action<object>
	// System.Action<short,short>
	// System.Action<uint,byte>
	// System.Action<uint,double>
	// System.Action<uint,int,uint>
	// System.Action<uint,int>
	// System.Action<uint,long>
	// System.Action<uint,object,object,object,object>
	// System.Action<uint,object,object>
	// System.Action<uint,object,uint>
	// System.Action<uint,object>
	// System.Action<uint,uint,byte>
	// System.Action<uint,uint,int>
	// System.Action<uint,uint,object>
	// System.Action<uint,uint,uint,byte>
	// System.Action<uint,uint,uint,int,uint>
	// System.Action<uint,uint,uint,int>
	// System.Action<uint,uint,uint,object>
	// System.Action<uint,uint,uint,uint,uint,object>
	// System.Action<uint,uint,uint,uint,uint,uint,uint,uint>
	// System.Action<uint,uint,uint,uint,uint,uint>
	// System.Action<uint,uint,uint,uint,uint>
	// System.Action<uint,uint,uint,uint>
	// System.Action<uint,uint,uint>
	// System.Action<uint,uint,ulong>
	// System.Action<uint,uint>
	// System.Action<uint,ulong>
	// System.Action<uint,ushort>
	// System.Action<uint>
	// System.Action<ushort,ushort,ushort>
	// System.Action<ushort>
	// System.ArraySegment.ArraySegmentEnumerator<byte>
	// System.ArraySegment.ArraySegmentEnumerator<int>
	// System.ArraySegment<byte>
	// System.ArraySegment<int>
	// System.Collections.Concurrent.ConcurrentDictionary.<GetEnumerator>d__32<System.Guid,object>
	// System.Collections.Concurrent.ConcurrentDictionary.DictionaryEnumerator<System.Guid,object>
	// System.Collections.Concurrent.ConcurrentDictionary.Node<System.Guid,object>
	// System.Collections.Concurrent.ConcurrentDictionary.Tables<System.Guid,object>
	// System.Collections.Concurrent.ConcurrentDictionary<System.Guid,object>
	// System.Collections.Generic.ArraySortHelper<CryPrinter.Pixel>
	// System.Collections.Generic.ArraySortHelper<Loom.DelayedQueueItem>
	// System.Collections.Generic.ArraySortHelper<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.ArraySortHelper<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.ArraySortHelper<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.ArraySortHelper<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.ArraySortHelper<System.Guid>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector2>
	// System.Collections.Generic.ArraySortHelper<byte>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<long>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.ArraySortHelper<uint>
	// System.Collections.Generic.ArraySortHelper<ushort>
	// System.Collections.Generic.Comparer<CryPrinter.Pixel>
	// System.Collections.Generic.Comparer<Loom.DelayedQueueItem>
	// System.Collections.Generic.Comparer<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.Comparer<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.Comparer<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.Comparer<System.Guid>
	// System.Collections.Generic.Comparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.Comparer<UnityEngine.Vector2>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<long>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Comparer<uint>
	// System.Collections.Generic.Comparer<ushort>
	// System.Collections.Generic.Dictionary.Enumerator<int,RouletteDeluxe.OriginalBitmapFont.Glyph>
	// System.Collections.Generic.Dictionary.Enumerator<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,uint>
	// System.Collections.Generic.Dictionary.Enumerator<object,RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Collections.Generic.Dictionary.Enumerator<object,byte>
	// System.Collections.Generic.Dictionary.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.Enumerator<uint,uint>
	// System.Collections.Generic.Dictionary.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,RouletteDeluxe.OriginalBitmapFont.Glyph>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,uint>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,byte>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<uint,uint>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,RouletteDeluxe.OriginalBitmapFont.Glyph>
	// System.Collections.Generic.Dictionary.KeyCollection<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.KeyCollection<int,byte>
	// System.Collections.Generic.Dictionary.KeyCollection<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<int,long>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,uint>
	// System.Collections.Generic.Dictionary.KeyCollection<object,RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Collections.Generic.Dictionary.KeyCollection<object,byte>
	// System.Collections.Generic.Dictionary.KeyCollection<object,float>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<uint,object>
	// System.Collections.Generic.Dictionary.KeyCollection<uint,uint>
	// System.Collections.Generic.Dictionary.KeyCollection<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,RouletteDeluxe.OriginalBitmapFont.Glyph>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,byte>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,long>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,uint>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,byte>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,float>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<uint,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<uint,uint>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ulong,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,RouletteDeluxe.OriginalBitmapFont.Glyph>
	// System.Collections.Generic.Dictionary.ValueCollection<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary.ValueCollection<int,byte>
	// System.Collections.Generic.Dictionary.ValueCollection<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<int,long>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,uint>
	// System.Collections.Generic.Dictionary.ValueCollection<object,RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Collections.Generic.Dictionary.ValueCollection<object,byte>
	// System.Collections.Generic.Dictionary.ValueCollection<object,float>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<uint,object>
	// System.Collections.Generic.Dictionary.ValueCollection<uint,uint>
	// System.Collections.Generic.Dictionary.ValueCollection<ulong,object>
	// System.Collections.Generic.Dictionary<int,RouletteDeluxe.OriginalBitmapFont.Glyph>
	// System.Collections.Generic.Dictionary<int,UnityEngine.Vector2>
	// System.Collections.Generic.Dictionary<int,byte>
	// System.Collections.Generic.Dictionary<int,int>
	// System.Collections.Generic.Dictionary<int,long>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<int,uint>
	// System.Collections.Generic.Dictionary<object,RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary<object,float>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<uint,object>
	// System.Collections.Generic.Dictionary<uint,uint>
	// System.Collections.Generic.Dictionary<ulong,object>
	// System.Collections.Generic.EqualityComparer<CryPrinter.Pixel>
	// System.Collections.Generic.EqualityComparer<Loom.DelayedQueueItem>
	// System.Collections.Generic.EqualityComparer<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.EqualityComparer<RouletteDeluxe.OriginalBitmapFont.Glyph>
	// System.Collections.Generic.EqualityComparer<RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Collections.Generic.EqualityComparer<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.EqualityComparer<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.EqualityComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.EqualityComparer<System.Guid>
	// System.Collections.Generic.EqualityComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<double>
	// System.Collections.Generic.EqualityComparer<float>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<long>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.EqualityComparer<uint>
	// System.Collections.Generic.EqualityComparer<ulong>
	// System.Collections.Generic.EqualityComparer<ushort>
	// System.Collections.Generic.HashSet.Enumerator<int>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet.Enumerator<uint>
	// System.Collections.Generic.HashSet<int>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSet<uint>
	// System.Collections.Generic.HashSetEqualityComparer<int>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.HashSetEqualityComparer<uint>
	// System.Collections.Generic.ICollection<CryPrinter.Pixel>
	// System.Collections.Generic.ICollection<Loom.DelayedQueueItem>
	// System.Collections.Generic.ICollection<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.ICollection<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.ICollection<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,RouletteDeluxe.OriginalBitmapFont.Glyph>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,RouletteDeluxe.OriginalSpriteAlignment.Rectangle>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.ICollection<System.Guid>
	// System.Collections.Generic.ICollection<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ICollection<UnityEngine.Vector2>
	// System.Collections.Generic.ICollection<byte>
	// System.Collections.Generic.ICollection<double>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<long>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.ICollection<uint>
	// System.Collections.Generic.ICollection<ushort>
	// System.Collections.Generic.IComparer<CryPrinter.Pixel>
	// System.Collections.Generic.IComparer<Loom.DelayedQueueItem>
	// System.Collections.Generic.IComparer<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.IComparer<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.IComparer<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IComparer<System.Guid>
	// System.Collections.Generic.IComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IComparer<UnityEngine.Vector2>
	// System.Collections.Generic.IComparer<byte>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<long>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IComparer<uint>
	// System.Collections.Generic.IComparer<ushort>
	// System.Collections.Generic.IDictionary<System.Guid,object>
	// System.Collections.Generic.IDictionary<int,long>
	// System.Collections.Generic.IDictionary<int,object>
	// System.Collections.Generic.IEnumerable<CryPrinter.Pixel>
	// System.Collections.Generic.IEnumerable<Loom.DelayedQueueItem>
	// System.Collections.Generic.IEnumerable<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.IEnumerable<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.IEnumerable<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.Guid,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,RouletteDeluxe.OriginalBitmapFont.Glyph>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,RouletteDeluxe.OriginalSpriteAlignment.Rectangle>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerable<System.Guid>
	// System.Collections.Generic.IEnumerable<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector2>
	// System.Collections.Generic.IEnumerable<byte>
	// System.Collections.Generic.IEnumerable<double>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<long>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerable<uint>
	// System.Collections.Generic.IEnumerable<ushort>
	// System.Collections.Generic.IEnumerator<CryPrinter.Pixel>
	// System.Collections.Generic.IEnumerator<Loom.DelayedQueueItem>
	// System.Collections.Generic.IEnumerator<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.IEnumerator<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.IEnumerator<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<System.Guid,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,RouletteDeluxe.OriginalBitmapFont.Glyph>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,uint>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,RouletteDeluxe.OriginalSpriteAlignment.Rectangle>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,float>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<uint,uint>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ulong,object>>
	// System.Collections.Generic.IEnumerator<System.Guid>
	// System.Collections.Generic.IEnumerator<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector2>
	// System.Collections.Generic.IEnumerator<byte>
	// System.Collections.Generic.IEnumerator<double>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<long>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEnumerator<uint>
	// System.Collections.Generic.IEnumerator<ushort>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IEqualityComparer<System.Guid>
	// System.Collections.Generic.IEqualityComparer<byte>
	// System.Collections.Generic.IEqualityComparer<double>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<long>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IEqualityComparer<uint>
	// System.Collections.Generic.IEqualityComparer<ulong>
	// System.Collections.Generic.IList<CryPrinter.Pixel>
	// System.Collections.Generic.IList<Loom.DelayedQueueItem>
	// System.Collections.Generic.IList<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.IList<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.IList<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.IList<System.Guid>
	// System.Collections.Generic.IList<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.IList<UnityEngine.Vector2>
	// System.Collections.Generic.IList<byte>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<long>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.IList<uint>
	// System.Collections.Generic.IList<ushort>
	// System.Collections.Generic.IReadOnlyCollection<object>
	// System.Collections.Generic.IReadOnlyList<object>
	// System.Collections.Generic.KeyValuePair<System.Guid,object>
	// System.Collections.Generic.KeyValuePair<int,RouletteDeluxe.OriginalBitmapFont.Glyph>
	// System.Collections.Generic.KeyValuePair<int,UnityEngine.Vector2>
	// System.Collections.Generic.KeyValuePair<int,byte>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<int,long>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<int,uint>
	// System.Collections.Generic.KeyValuePair<object,RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Collections.Generic.KeyValuePair<object,byte>
	// System.Collections.Generic.KeyValuePair<object,float>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<object,uint>
	// System.Collections.Generic.KeyValuePair<object,ushort>
	// System.Collections.Generic.KeyValuePair<uint,byte>
	// System.Collections.Generic.KeyValuePair<uint,object>
	// System.Collections.Generic.KeyValuePair<uint,uint>
	// System.Collections.Generic.KeyValuePair<ulong,object>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.List.Enumerator<CryPrinter.Pixel>
	// System.Collections.Generic.List.Enumerator<Loom.DelayedQueueItem>
	// System.Collections.Generic.List.Enumerator<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.List.Enumerator<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.List.Enumerator<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.List.Enumerator<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.List.Enumerator<System.Guid>
	// System.Collections.Generic.List.Enumerator<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector2>
	// System.Collections.Generic.List.Enumerator<byte>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<long>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List.Enumerator<uint>
	// System.Collections.Generic.List.Enumerator<ushort>
	// System.Collections.Generic.List.SynchronizedList<CryPrinter.Pixel>
	// System.Collections.Generic.List.SynchronizedList<Loom.DelayedQueueItem>
	// System.Collections.Generic.List.SynchronizedList<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.List.SynchronizedList<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.List.SynchronizedList<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.List.SynchronizedList<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.List.SynchronizedList<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.List.SynchronizedList<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.List.SynchronizedList<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List.SynchronizedList<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.List.SynchronizedList<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.List.SynchronizedList<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.List.SynchronizedList<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.List.SynchronizedList<System.Guid>
	// System.Collections.Generic.List.SynchronizedList<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List.SynchronizedList<UnityEngine.Vector2>
	// System.Collections.Generic.List.SynchronizedList<byte>
	// System.Collections.Generic.List.SynchronizedList<int>
	// System.Collections.Generic.List.SynchronizedList<long>
	// System.Collections.Generic.List.SynchronizedList<object>
	// System.Collections.Generic.List.SynchronizedList<uint>
	// System.Collections.Generic.List.SynchronizedList<ushort>
	// System.Collections.Generic.List<CryPrinter.Pixel>
	// System.Collections.Generic.List<Loom.DelayedQueueItem>
	// System.Collections.Generic.List<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.List<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.List<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.List<System.Guid>
	// System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.List<UnityEngine.Vector2>
	// System.Collections.Generic.List<byte>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<long>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.List<uint>
	// System.Collections.Generic.List<ushort>
	// System.Collections.Generic.ObjectComparer<CryPrinter.Pixel>
	// System.Collections.Generic.ObjectComparer<Loom.DelayedQueueItem>
	// System.Collections.Generic.ObjectComparer<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.ObjectComparer<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.ObjectComparer<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.ObjectComparer<System.Guid>
	// System.Collections.Generic.ObjectComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<long>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectComparer<uint>
	// System.Collections.Generic.ObjectComparer<ushort>
	// System.Collections.Generic.ObjectEqualityComparer<CryPrinter.Pixel>
	// System.Collections.Generic.ObjectEqualityComparer<Loom.DelayedQueueItem>
	// System.Collections.Generic.ObjectEqualityComparer<Loom.NoDelayedQueueItem>
	// System.Collections.Generic.ObjectEqualityComparer<RouletteDeluxe.OriginalBitmapFont.Glyph>
	// System.Collections.Generic.ObjectEqualityComparer<RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Collections.Generic.ObjectEqualityComparer<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.Generic.ObjectEqualityComparer<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.Guid>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector2>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<double>
	// System.Collections.Generic.ObjectEqualityComparer<float>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<long>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<uint>
	// System.Collections.Generic.ObjectEqualityComparer<ulong>
	// System.Collections.Generic.ObjectEqualityComparer<ushort>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<object,int>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<object,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<object,uint>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<object,ushort>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<uint,byte>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<uint,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<object,int>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<object,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<object,uint>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<object,ushort>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<uint,byte>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<uint,object>
	// System.Collections.Generic.SortedDictionary.Enumerator<object,int>
	// System.Collections.Generic.SortedDictionary.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.Enumerator<object,uint>
	// System.Collections.Generic.SortedDictionary.Enumerator<object,ushort>
	// System.Collections.Generic.SortedDictionary.Enumerator<uint,byte>
	// System.Collections.Generic.SortedDictionary.Enumerator<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<object,int>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<object,uint>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<object,ushort>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<uint,byte>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<object,int>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<object,uint>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<object,ushort>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<uint,byte>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_1<object,int>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_1<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_1<object,uint>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_1<object,ushort>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_1<uint,byte>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_1<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<object,uint>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<object,ushort>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<uint,byte>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection<object,int>
	// System.Collections.Generic.SortedDictionary.KeyCollection<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection<object,uint>
	// System.Collections.Generic.SortedDictionary.KeyCollection<object,ushort>
	// System.Collections.Generic.SortedDictionary.KeyCollection<uint,byte>
	// System.Collections.Generic.SortedDictionary.KeyCollection<uint,object>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<object,int>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<object,object>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<object,uint>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<object,ushort>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<uint,byte>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<uint,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<object,int>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<object,uint>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<object,ushort>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<uint,byte>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<uint,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<object,int>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<object,uint>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<object,ushort>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<uint,byte>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<uint,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_1<object,int>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_1<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_1<object,uint>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_1<object,ushort>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_1<uint,byte>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_1<uint,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<object,uint>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<object,ushort>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<uint,byte>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<uint,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<object,int>
	// System.Collections.Generic.SortedDictionary.ValueCollection<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<object,uint>
	// System.Collections.Generic.SortedDictionary.ValueCollection<object,ushort>
	// System.Collections.Generic.SortedDictionary.ValueCollection<uint,byte>
	// System.Collections.Generic.SortedDictionary.ValueCollection<uint,object>
	// System.Collections.Generic.SortedDictionary<object,int>
	// System.Collections.Generic.SortedDictionary<object,object>
	// System.Collections.Generic.SortedDictionary<object,uint>
	// System.Collections.Generic.SortedDictionary<object,ushort>
	// System.Collections.Generic.SortedDictionary<uint,byte>
	// System.Collections.Generic.SortedDictionary<uint,object>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<int>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<int>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_1<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_1<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_1<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_1<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_1<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_1<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_1<int>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass85_0<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass85_0<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass85_0<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass85_0<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass85_0<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass85_0<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass85_0<int>
	// System.Collections.Generic.SortedSet.<Reverse>d__94<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSet.<Reverse>d__94<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.<Reverse>d__94<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSet.<Reverse>d__94<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSet.<Reverse>d__94<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSet.<Reverse>d__94<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.<Reverse>d__94<int>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.Enumerator<int>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.Node<int>
	// System.Collections.Generic.SortedSet.TreeSubSet.<>c__DisplayClass9_0<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSet.TreeSubSet.<>c__DisplayClass9_0<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.TreeSubSet.<>c__DisplayClass9_0<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSet.TreeSubSet.<>c__DisplayClass9_0<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSet.TreeSubSet.<>c__DisplayClass9_0<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSet.TreeSubSet.<>c__DisplayClass9_0<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.TreeSubSet.<>c__DisplayClass9_0<int>
	// System.Collections.Generic.SortedSet.TreeSubSet<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSet.TreeSubSet<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.TreeSubSet<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSet.TreeSubSet<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSet.TreeSubSet<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSet.TreeSubSet<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet.TreeSubSet<int>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSet<int>
	// System.Collections.Generic.SortedSetEqualityComparer<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.SortedSetEqualityComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSetEqualityComparer<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.SortedSetEqualityComparer<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.SortedSetEqualityComparer<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.SortedSetEqualityComparer<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.SortedSetEqualityComparer<int>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack.Enumerator<uint>
	// System.Collections.Generic.Stack<object>
	// System.Collections.Generic.Stack<uint>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.Generic.TreeWalkPredicate<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<CryPrinter.Pixel>
	// System.Collections.ObjectModel.ReadOnlyCollection<Loom.DelayedQueueItem>
	// System.Collections.ObjectModel.ReadOnlyCollection<Loom.NoDelayedQueueItem>
	// System.Collections.ObjectModel.ReadOnlyCollection<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Collections.ObjectModel.ReadOnlyCollection<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.Guid>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.EventSystems.RaycastResult>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector2>
	// System.Collections.ObjectModel.ReadOnlyCollection<byte>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<long>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<uint>
	// System.Collections.ObjectModel.ReadOnlyCollection<ushort>
	// System.Collections.ObjectModel.ReadOnlyDictionary.DictionaryEnumerator<int,long>
	// System.Collections.ObjectModel.ReadOnlyDictionary.KeyCollection<int,long>
	// System.Collections.ObjectModel.ReadOnlyDictionary.ValueCollection<int,long>
	// System.Collections.ObjectModel.ReadOnlyDictionary<int,long>
	// System.Comparison<CryPrinter.Pixel>
	// System.Comparison<Loom.DelayedQueueItem>
	// System.Comparison<Loom.NoDelayedQueueItem>
	// System.Comparison<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Comparison<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Comparison<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Comparison<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Comparison<System.Guid>
	// System.Comparison<UnityEngine.EventSystems.RaycastResult>
	// System.Comparison<UnityEngine.Vector2>
	// System.Comparison<byte>
	// System.Comparison<int>
	// System.Comparison<long>
	// System.Comparison<object>
	// System.Comparison<uint>
	// System.Comparison<ushort>
	// System.Converter<byte,int>
	// System.Converter<int,object>
	// System.Converter<object,object>
	// System.EventHandler<object>
	// System.Func<Loom.DelayedQueueItem,byte>
	// System.Func<RouletteDeluxe.OriginalCashoutConstructorResources.Size>
	// System.Func<RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Func<RouletteDeluxe.OriginalVideoInputDispatch.ClockSample>
	// System.Func<System.Collections.Generic.KeyValuePair<int,long>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<int,long>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<int,long>,object>
	// System.Func<System.Collections.Generic.KeyValuePair<int,object>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<int,object>,int>
	// System.Func<System.Collections.Generic.KeyValuePair<int,object>,object>
	// System.Func<System.Guid,object,object>
	// System.Func<System.Guid,object>
	// System.Func<UnityEngine.Color,UnityEngine.Color,float,UnityEngine.Color>
	// System.Func<UnityEngine.EventSystems.RaycastResult,byte>
	// System.Func<UnityEngine.Vector3,UnityEngine.Vector3,float,UnityEngine.Vector3>
	// System.Func<byte,byte>
	// System.Func<byte,int>
	// System.Func<byte,object,object>
	// System.Func<byte,object>
	// System.Func<byte,uint,object>
	// System.Func<byte>
	// System.Func<double,byte>
	// System.Func<int,byte,int>
	// System.Func<int,byte>
	// System.Func<int,int,int>
	// System.Func<int,int>
	// System.Func<int,long,byte>
	// System.Func<int,long,int>
	// System.Func<int,long>
	// System.Func<int,object,object,int>
	// System.Func<int,object,object>
	// System.Func<int,object,short>
	// System.Func<int,object,uint,int,object>
	// System.Func<int,object,uint,int>
	// System.Func<int,object,uint>
	// System.Func<int,object>
	// System.Func<int,short>
	// System.Func<int,uint,int,int>
	// System.Func<int,uint,int>
	// System.Func<int,uint,object>
	// System.Func<int,uint,uint,int>
	// System.Func<int,uint,uint,uint,uint>
	// System.Func<int,uint,uint>
	// System.Func<int,uint>
	// System.Func<int>
	// System.Func<long,byte,int,byte,object>
	// System.Func<long,byte>
	// System.Func<long,long,byte>
	// System.Func<long,object,object>
	// System.Func<long,object>
	// System.Func<long>
	// System.Func<object,System.Nullable<RouletteDeluxe.OriginalSpriteAlignment.Rectangle>>
	// System.Func<object,System.Nullable<int>,object,object>
	// System.Func<object,byte>
	// System.Func<object,double>
	// System.Func<object,int,byte>
	// System.Func<object,int,int>
	// System.Func<object,int,object,object>
	// System.Func<object,int,object,short>
	// System.Func<object,int,object>
	// System.Func<object,int,uint,object>
	// System.Func<object,int>
	// System.Func<object,long,int>
	// System.Func<object,long,long>
	// System.Func<object,long>
	// System.Func<object,object,byte,object>
	// System.Func<object,object,byte>
	// System.Func<object,object,int>
	// System.Func<object,object,object,byte>
	// System.Func<object,object,object,int>
	// System.Func<object,object,object,uint>
	// System.Func<object,object,object>
	// System.Func<object,object,short>
	// System.Func<object,object,uint,byte>
	// System.Func<object,object,uint,object>
	// System.Func<object,object,uint,uint,uint,object>
	// System.Func<object,object>
	// System.Func<object,short>
	// System.Func<object,uint,byte,byte>
	// System.Func<object,uint,byte>
	// System.Func<object,uint,int>
	// System.Func<object,uint,object>
	// System.Func<object,uint,uint,int>
	// System.Func<object,uint,uint>
	// System.Func<object,uint>
	// System.Func<object>
	// System.Func<uint,RouletteDeluxe.OriginalCashoutConstructorResources.Bounds>
	// System.Func<uint,byte,uint,uint>
	// System.Func<uint,byte>
	// System.Func<uint,double>
	// System.Func<uint,int,int>
	// System.Func<uint,int,uint,uint>
	// System.Func<uint,int,uint>
	// System.Func<uint,int>
	// System.Func<uint,object,object>
	// System.Func<uint,object,uint>
	// System.Func<uint,object,ulong>
	// System.Func<uint,object>
	// System.Func<uint,uint,byte>
	// System.Func<uint,uint,int,int,int,int,int,int>
	// System.Func<uint,uint,int,int,uint>
	// System.Func<uint,uint,int,int>
	// System.Func<uint,uint,int,object>
	// System.Func<uint,uint,int>
	// System.Func<uint,uint,object,object>
	// System.Func<uint,uint,object,uint>
	// System.Func<uint,uint,object>
	// System.Func<uint,uint,uint,byte,byte>
	// System.Func<uint,uint,uint,byte,uint>
	// System.Func<uint,uint,uint,int>
	// System.Func<uint,uint,uint,uint,byte>
	// System.Func<uint,uint,uint,uint,uint,byte>
	// System.Func<uint,uint,uint,uint,uint>
	// System.Func<uint,uint,uint,uint>
	// System.Func<uint,uint,uint>
	// System.Func<uint,uint,ulong>
	// System.Func<uint,uint>
	// System.Func<uint,ushort>
	// System.Func<uint>
	// System.Func<ulong>
	// System.Func<ushort,byte>
	// System.IComparable<RouletteDeluxe.OriginalFeatureFloat80>
	// System.Linq.Buffer<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Linq.Buffer<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Linq.Buffer<byte>
	// System.Linq.Buffer<double>
	// System.Linq.Buffer<int>
	// System.Linq.Buffer<object>
	// System.Linq.Buffer<ushort>
	// System.Linq.Enumerable.<CastIterator>d__99<int>
	// System.Linq.Enumerable.<CastIterator>d__99<object>
	// System.Linq.Enumerable.<ConcatIterator>d__59<int>
	// System.Linq.Enumerable.<ConcatIterator>d__59<object>
	// System.Linq.Enumerable.<DistinctIterator>d__68<int>
	// System.Linq.Enumerable.<DistinctIterator>d__68<object>
	// System.Linq.Enumerable.<RepeatIterator>d__117<long>
	// System.Linq.Enumerable.<RepeatIterator>d__117<object>
	// System.Linq.Enumerable.<ReverseIterator>d__79<object>
	// System.Linq.Enumerable.<SelectIterator>d__5<object,object>
	// System.Linq.Enumerable.<SelectManyIterator>d__17<object,byte>
	// System.Linq.Enumerable.<SkipIterator>d__31<byte>
	// System.Linq.Enumerable.<SkipIterator>d__31<object>
	// System.Linq.Enumerable.<TakeIterator>d__25<byte>
	// System.Linq.Enumerable.<TakeIterator>d__25<int>
	// System.Linq.Enumerable.<TakeIterator>d__25<object>
	// System.Linq.Enumerable.<TakeIterator>d__25<ushort>
	// System.Linq.Enumerable.<TakeWhileIterator>d__27<byte>
	// System.Linq.Enumerable.<WhereIterator>d__2<object>
	// System.Linq.Enumerable.<ZipIterator>d__65<object,object,byte>
	// System.Linq.Enumerable.Iterator<Loom.DelayedQueueItem>
	// System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Linq.Enumerable.Iterator<byte>
	// System.Linq.Enumerable.Iterator<double>
	// System.Linq.Enumerable.Iterator<int>
	// System.Linq.Enumerable.Iterator<long>
	// System.Linq.Enumerable.Iterator<object>
	// System.Linq.Enumerable.WhereArrayIterator<Loom.DelayedQueueItem>
	// System.Linq.Enumerable.WhereArrayIterator<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Linq.Enumerable.WhereArrayIterator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Linq.Enumerable.WhereArrayIterator<object>
	// System.Linq.Enumerable.WhereEnumerableIterator<Loom.DelayedQueueItem>
	// System.Linq.Enumerable.WhereEnumerableIterator<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Linq.Enumerable.WhereEnumerableIterator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Linq.Enumerable.WhereEnumerableIterator<byte>
	// System.Linq.Enumerable.WhereEnumerableIterator<double>
	// System.Linq.Enumerable.WhereEnumerableIterator<int>
	// System.Linq.Enumerable.WhereEnumerableIterator<long>
	// System.Linq.Enumerable.WhereEnumerableIterator<object>
	// System.Linq.Enumerable.WhereListIterator<Loom.DelayedQueueItem>
	// System.Linq.Enumerable.WhereListIterator<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Linq.Enumerable.WhereListIterator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Linq.Enumerable.WhereListIterator<object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<System.Collections.Generic.KeyValuePair<int,long>,object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<System.Collections.Generic.KeyValuePair<int,object>,int>
	// System.Linq.Enumerable.WhereSelectArrayIterator<System.Collections.Generic.KeyValuePair<int,object>,object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<byte,int>
	// System.Linq.Enumerable.WhereSelectArrayIterator<int,byte>
	// System.Linq.Enumerable.WhereSelectArrayIterator<int,int>
	// System.Linq.Enumerable.WhereSelectArrayIterator<int,object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<long,object>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,double>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,int>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,long>
	// System.Linq.Enumerable.WhereSelectArrayIterator<object,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<System.Collections.Generic.KeyValuePair<int,long>,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<System.Collections.Generic.KeyValuePair<int,object>,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<System.Collections.Generic.KeyValuePair<int,object>,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<byte,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<int,byte>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<int,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<int,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<long,object>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,double>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,int>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,long>
	// System.Linq.Enumerable.WhereSelectEnumerableIterator<object,object>
	// System.Linq.Enumerable.WhereSelectListIterator<System.Collections.Generic.KeyValuePair<int,long>,object>
	// System.Linq.Enumerable.WhereSelectListIterator<System.Collections.Generic.KeyValuePair<int,object>,int>
	// System.Linq.Enumerable.WhereSelectListIterator<System.Collections.Generic.KeyValuePair<int,object>,object>
	// System.Linq.Enumerable.WhereSelectListIterator<byte,int>
	// System.Linq.Enumerable.WhereSelectListIterator<int,byte>
	// System.Linq.Enumerable.WhereSelectListIterator<int,int>
	// System.Linq.Enumerable.WhereSelectListIterator<int,object>
	// System.Linq.Enumerable.WhereSelectListIterator<long,object>
	// System.Linq.Enumerable.WhereSelectListIterator<object,double>
	// System.Linq.Enumerable.WhereSelectListIterator<object,int>
	// System.Linq.Enumerable.WhereSelectListIterator<object,long>
	// System.Linq.Enumerable.WhereSelectListIterator<object,object>
	// System.Linq.EnumerableSorter<System.Collections.Generic.KeyValuePair<int,long>,int>
	// System.Linq.EnumerableSorter<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Linq.EnumerableSorter<System.Collections.Generic.KeyValuePair<int,object>,int>
	// System.Linq.EnumerableSorter<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Linq.EnumerableSorter<int,int>
	// System.Linq.EnumerableSorter<int>
	// System.Linq.EnumerableSorter<object,int>
	// System.Linq.EnumerableSorter<object,object>
	// System.Linq.EnumerableSorter<object>
	// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Linq.IOrderedEnumerable<int>
	// System.Linq.IdentityFunction.<>c<object>
	// System.Linq.IdentityFunction<object>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<int>
	// System.Linq.OrderedEnumerable.<GetEnumerator>d__1<object>
	// System.Linq.OrderedEnumerable<System.Collections.Generic.KeyValuePair<int,long>,int>
	// System.Linq.OrderedEnumerable<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Linq.OrderedEnumerable<System.Collections.Generic.KeyValuePair<int,object>,int>
	// System.Linq.OrderedEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Linq.OrderedEnumerable<int,int>
	// System.Linq.OrderedEnumerable<int>
	// System.Linq.OrderedEnumerable<object,int>
	// System.Linq.OrderedEnumerable<object,object>
	// System.Linq.OrderedEnumerable<object>
	// System.Linq.Set<int>
	// System.Linq.Set<object>
	// System.Nullable<RouletteDeluxe.OriginalSpriteAlignment.Rectangle>
	// System.Nullable<UnityEngine.Vector2>
	// System.Nullable<byte>
	// System.Nullable<float>
	// System.Nullable<int>
	// System.Nullable<long>
	// System.Predicate<CryPrinter.Pixel>
	// System.Predicate<Loom.DelayedQueueItem>
	// System.Predicate<Loom.NoDelayedQueueItem>
	// System.Predicate<RouletteDeluxe.OriginalSpriteInputTarget.Point>
	// System.Predicate<RouletteDeluxe.RouletteGame.FailurePromptBoundary>
	// System.Predicate<System.Collections.Generic.KeyValuePair<int,long>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<object,uint>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<object,ushort>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<uint,byte>>
	// System.Predicate<System.Collections.Generic.KeyValuePair<uint,object>>
	// System.Predicate<System.Guid>
	// System.Predicate<UnityEngine.EventSystems.RaycastResult>
	// System.Predicate<UnityEngine.Vector2>
	// System.Predicate<byte>
	// System.Predicate<int>
	// System.Predicate<long>
	// System.Predicate<object>
	// System.Predicate<uint>
	// System.Predicate<ushort>
	// System.ReadOnlySpan<byte>
	// System.ReadOnlySpan<int>
	// System.Span.Enumerator<byte>
	// System.Span.Enumerator<int>
	// System.Span<byte>
	// System.Span<int>
	// UnityEngine.EventSystems.ExecuteEvents.EventFunction<object>
	// UnityEngine.Events.InvokableCall<byte>
	// UnityEngine.Events.InvokableCall<float>
	// UnityEngine.Events.InvokableCall<object>
	// UnityEngine.Events.UnityAction<System.DateTime>
	// UnityEngine.Events.UnityAction<byte>
	// UnityEngine.Events.UnityAction<float>
	// UnityEngine.Events.UnityAction<int>
	// UnityEngine.Events.UnityAction<object>
	// UnityEngine.Events.UnityAction<ulong>
	// UnityEngine.Events.UnityEvent<byte>
	// UnityEngine.Events.UnityEvent<float>
	// UnityEngine.Events.UnityEvent<object>
	// UnityEngine.UI.ListPool<object>
	// UnityEngine.UI.ObjectPool<object>
	// ZXing.BarcodeWriter<object>
	// ZXing.Rendering.IBarcodeRenderer<object>
	// }}

	public void RefMethods()
	{
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string,Newtonsoft.Json.JsonSerializerSettings)
		// object System.Activator.CreateInstance<object>()
		// int[] System.Array.ConvertAll<byte,int>(byte[],System.Converter<byte,int>)
		// byte[] System.Array.Empty<byte>()
		// object[] System.Array.Empty<object>()
		// uint[] System.Array.Empty<uint>()
		// object System.Array.Find<object>(object[],System.Predicate<object>)
		// int System.Array.IndexOf<byte>(byte[],byte)
		// int System.Array.IndexOf<byte>(byte[],byte,int)
		// int System.Array.IndexOf<byte>(byte[],byte,int,int)
		// int System.Array.IndexOf<int>(int[],int)
		// int System.Array.IndexOf<object>(object[],object)
		// int System.Array.IndexOf<uint>(uint[],uint)
		// int System.Array.IndexOfImpl<byte>(byte[],byte,int,int)
		// int System.Array.IndexOfImpl<int>(int[],int,int,int)
		// int System.Array.IndexOfImpl<object>(object[],object,int,int)
		// int System.Array.IndexOfImpl<uint>(uint[],uint,int,int)
		// int System.Array.LastIndexOf<byte>(byte[],byte)
		// int System.Array.LastIndexOf<byte>(byte[],byte,int,int)
		// int System.Array.LastIndexOfImpl<byte>(byte[],byte,int,int)
		// System.Void System.Array.Resize<byte>(byte[]&,int)
		// System.Void System.Array.Sort<object>(object[],System.Comparison<object>)
		// System.Collections.Generic.List<object> System.Collections.Generic.List<int>.ConvertAll<object>(System.Converter<int,object>)
		// System.Collections.Generic.List<object> System.Collections.Generic.List<object>.ConvertAll<object>(System.Converter<object,object>)
		// bool System.Linq.Enumerable.All<System.Collections.Generic.KeyValuePair<int,long>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,long>>,System.Func<System.Collections.Generic.KeyValuePair<int,long>,bool>)
		// bool System.Linq.Enumerable.All<System.Collections.Generic.KeyValuePair<int,object>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>,System.Func<System.Collections.Generic.KeyValuePair<int,object>,bool>)
		// bool System.Linq.Enumerable.All<UnityEngine.EventSystems.RaycastResult>(System.Collections.Generic.IEnumerable<UnityEngine.EventSystems.RaycastResult>,System.Func<UnityEngine.EventSystems.RaycastResult,bool>)
		// bool System.Linq.Enumerable.All<byte>(System.Collections.Generic.IEnumerable<byte>,System.Func<byte,bool>)
		// bool System.Linq.Enumerable.All<long>(System.Collections.Generic.IEnumerable<long>,System.Func<long,bool>)
		// bool System.Linq.Enumerable.All<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// bool System.Linq.Enumerable.All<ushort>(System.Collections.Generic.IEnumerable<ushort>,System.Func<ushort,bool>)
		// bool System.Linq.Enumerable.Any<System.Collections.Generic.KeyValuePair<int,long>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,long>>,System.Func<System.Collections.Generic.KeyValuePair<int,long>,bool>)
		// bool System.Linq.Enumerable.Any<System.Collections.Generic.KeyValuePair<int,object>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>,System.Func<System.Collections.Generic.KeyValuePair<int,object>,bool>)
		// bool System.Linq.Enumerable.Any<UnityEngine.EventSystems.RaycastResult>(System.Collections.Generic.IEnumerable<UnityEngine.EventSystems.RaycastResult>,System.Func<UnityEngine.EventSystems.RaycastResult,bool>)
		// bool System.Linq.Enumerable.Any<int>(System.Collections.Generic.IEnumerable<int>,System.Func<int,bool>)
		// bool System.Linq.Enumerable.Any<long>(System.Collections.Generic.IEnumerable<long>,System.Func<long,bool>)
		// bool System.Linq.Enumerable.Any<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// bool System.Linq.Enumerable.Any<ushort>(System.Collections.Generic.IEnumerable<ushort>,System.Func<ushort,bool>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Cast<int>(System.Collections.IEnumerable)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Cast<object>(System.Collections.IEnumerable)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.CastIterator<int>(System.Collections.IEnumerable)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.CastIterator<object>(System.Collections.IEnumerable)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Concat<int>(System.Collections.Generic.IEnumerable<int>,System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Concat<object>(System.Collections.Generic.IEnumerable<object>,System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.ConcatIterator<int>(System.Collections.Generic.IEnumerable<int>,System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.ConcatIterator<object>(System.Collections.Generic.IEnumerable<object>,System.Collections.Generic.IEnumerable<object>)
		// bool System.Linq.Enumerable.Contains<int>(System.Collections.Generic.IEnumerable<int>,int)
		// bool System.Linq.Enumerable.Contains<int>(System.Collections.Generic.IEnumerable<int>,int,System.Collections.Generic.IEqualityComparer<int>)
		// bool System.Linq.Enumerable.Contains<object>(System.Collections.Generic.IEnumerable<object>,object)
		// bool System.Linq.Enumerable.Contains<object>(System.Collections.Generic.IEnumerable<object>,object,System.Collections.Generic.IEqualityComparer<object>)
		// int System.Linq.Enumerable.Count<int>(System.Collections.Generic.IEnumerable<int>)
		// int System.Linq.Enumerable.Count<int>(System.Collections.Generic.IEnumerable<int>,System.Func<int,bool>)
		// int System.Linq.Enumerable.Count<object>(System.Collections.Generic.IEnumerable<object>)
		// int System.Linq.Enumerable.Count<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Distinct<int>(System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Distinct<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.DistinctIterator<int>(System.Collections.Generic.IEnumerable<int>,System.Collections.Generic.IEqualityComparer<int>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.DistinctIterator<object>(System.Collections.Generic.IEnumerable<object>,System.Collections.Generic.IEqualityComparer<object>)
		// object System.Linq.Enumerable.ElementAt<object>(System.Collections.Generic.IEnumerable<object>,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Empty<object>()
		// int System.Linq.Enumerable.First<int>(System.Collections.Generic.IEnumerable<int>)
		// object System.Linq.Enumerable.First<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// int System.Linq.Enumerable.Last<int>(System.Collections.Generic.IEnumerable<int>)
		// object System.Linq.Enumerable.Last<object>(System.Collections.Generic.IEnumerable<object>)
		// object System.Linq.Enumerable.Last<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<int,long>> System.Linq.Enumerable.OrderBy<System.Collections.Generic.KeyValuePair<int,long>,int>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,long>>,System.Func<System.Collections.Generic.KeyValuePair<int,long>,int>)
		// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<int,object>> System.Linq.Enumerable.OrderBy<System.Collections.Generic.KeyValuePair<int,object>,int>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>,System.Func<System.Collections.Generic.KeyValuePair<int,object>,int>)
		// System.Linq.IOrderedEnumerable<int> System.Linq.Enumerable.OrderBy<int,int>(System.Collections.Generic.IEnumerable<int>,System.Func<int,int>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderBy<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderBy<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Linq.IOrderedEnumerable<object> System.Linq.Enumerable.OrderByDescending<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<long> System.Linq.Enumerable.Repeat<long>(long,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Repeat<object>(object,int)
		// System.Collections.Generic.IEnumerable<long> System.Linq.Enumerable.RepeatIterator<long>(long,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.RepeatIterator<object>(object,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Reverse<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.ReverseIterator<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.Select<int,byte>(System.Collections.Generic.IEnumerable<int>,System.Func<int,byte>)
		// System.Collections.Generic.IEnumerable<double> System.Linq.Enumerable.Select<object,double>(System.Collections.Generic.IEnumerable<object>,System.Func<object,double>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<System.Collections.Generic.KeyValuePair<int,object>,int>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>,System.Func<System.Collections.Generic.KeyValuePair<int,object>,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<byte,int>(System.Collections.Generic.IEnumerable<byte>,System.Func<byte,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<int,int>(System.Collections.Generic.IEnumerable<int>,System.Func<int,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Select<object,int>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<long> System.Linq.Enumerable.Select<object,long>(System.Collections.Generic.IEnumerable<object>,System.Func<object,long>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<System.Collections.Generic.KeyValuePair<int,long>,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,long>>,System.Func<System.Collections.Generic.KeyValuePair<int,long>,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<System.Collections.Generic.KeyValuePair<int,object>,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>,System.Func<System.Collections.Generic.KeyValuePair<int,object>,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<int,object>(System.Collections.Generic.IEnumerable<int>,System.Func<int,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<long,object>(System.Collections.Generic.IEnumerable<long>,System.Func<long,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Select<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SelectIterator<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int,object>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.SelectMany<object,byte>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.Collections.Generic.IEnumerable<byte>>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.SelectManyIterator<object,byte>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.Collections.Generic.IEnumerable<byte>>)
		// bool System.Linq.Enumerable.SequenceEqual<byte>(System.Collections.Generic.IEnumerable<byte>,System.Collections.Generic.IEnumerable<byte>)
		// bool System.Linq.Enumerable.SequenceEqual<byte>(System.Collections.Generic.IEnumerable<byte>,System.Collections.Generic.IEnumerable<byte>,System.Collections.Generic.IEqualityComparer<byte>)
		// bool System.Linq.Enumerable.SequenceEqual<double>(System.Collections.Generic.IEnumerable<double>,System.Collections.Generic.IEnumerable<double>)
		// bool System.Linq.Enumerable.SequenceEqual<double>(System.Collections.Generic.IEnumerable<double>,System.Collections.Generic.IEnumerable<double>,System.Collections.Generic.IEqualityComparer<double>)
		// bool System.Linq.Enumerable.SequenceEqual<int>(System.Collections.Generic.IEnumerable<int>,System.Collections.Generic.IEnumerable<int>)
		// bool System.Linq.Enumerable.SequenceEqual<int>(System.Collections.Generic.IEnumerable<int>,System.Collections.Generic.IEnumerable<int>,System.Collections.Generic.IEqualityComparer<int>)
		// bool System.Linq.Enumerable.SequenceEqual<long>(System.Collections.Generic.IEnumerable<long>,System.Collections.Generic.IEnumerable<long>)
		// bool System.Linq.Enumerable.SequenceEqual<long>(System.Collections.Generic.IEnumerable<long>,System.Collections.Generic.IEnumerable<long>,System.Collections.Generic.IEqualityComparer<long>)
		// bool System.Linq.Enumerable.SequenceEqual<object>(System.Collections.Generic.IEnumerable<object>,System.Collections.Generic.IEnumerable<object>)
		// bool System.Linq.Enumerable.SequenceEqual<object>(System.Collections.Generic.IEnumerable<object>,System.Collections.Generic.IEnumerable<object>,System.Collections.Generic.IEqualityComparer<object>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.Skip<byte>(System.Collections.Generic.IEnumerable<byte>,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Skip<object>(System.Collections.Generic.IEnumerable<object>,int)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.SkipIterator<byte>(System.Collections.Generic.IEnumerable<byte>,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.SkipIterator<object>(System.Collections.Generic.IEnumerable<object>,int)
		// long System.Linq.Enumerable.Sum<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,long>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.Take<byte>(System.Collections.Generic.IEnumerable<byte>,int)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Take<int>(System.Collections.Generic.IEnumerable<int>,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Take<object>(System.Collections.Generic.IEnumerable<object>,int)
		// System.Collections.Generic.IEnumerable<ushort> System.Linq.Enumerable.Take<ushort>(System.Collections.Generic.IEnumerable<ushort>,int)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.TakeIterator<byte>(System.Collections.Generic.IEnumerable<byte>,int)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.TakeIterator<int>(System.Collections.Generic.IEnumerable<int>,int)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.TakeIterator<object>(System.Collections.Generic.IEnumerable<object>,int)
		// System.Collections.Generic.IEnumerable<ushort> System.Linq.Enumerable.TakeIterator<ushort>(System.Collections.Generic.IEnumerable<ushort>,int)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.TakeWhile<byte>(System.Collections.Generic.IEnumerable<byte>,System.Func<byte,bool>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.TakeWhileIterator<byte>(System.Collections.Generic.IEnumerable<byte>,System.Func<byte,bool>)
		// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<int,long>> System.Linq.Enumerable.ThenBy<System.Collections.Generic.KeyValuePair<int,long>,int>(System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<int,long>>,System.Func<System.Collections.Generic.KeyValuePair<int,long>,int>)
		// System.Linq.IOrderedEnumerable<int> System.Linq.Enumerable.ThenBy<int,int>(System.Linq.IOrderedEnumerable<int>,System.Func<int,int>)
		// byte[] System.Linq.Enumerable.ToArray<byte>(System.Collections.Generic.IEnumerable<byte>)
		// double[] System.Linq.Enumerable.ToArray<double>(System.Collections.Generic.IEnumerable<double>)
		// int[] System.Linq.Enumerable.ToArray<int>(System.Collections.Generic.IEnumerable<int>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// ushort[] System.Linq.Enumerable.ToArray<ushort>(System.Collections.Generic.IEnumerable<ushort>)
		// System.Collections.Generic.Dictionary<int,object> System.Linq.Enumerable.ToDictionary<System.Collections.Generic.KeyValuePair<int,object>,int,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>,System.Func<System.Collections.Generic.KeyValuePair<int,object>,int>,System.Func<System.Collections.Generic.KeyValuePair<int,object>,object>)
		// System.Collections.Generic.Dictionary<int,object> System.Linq.Enumerable.ToDictionary<System.Collections.Generic.KeyValuePair<int,object>,int,object>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>,System.Func<System.Collections.Generic.KeyValuePair<int,object>,int>,System.Func<System.Collections.Generic.KeyValuePair<int,object>,object>,System.Collections.Generic.IEqualityComparer<int>)
		// System.Collections.Generic.Dictionary<int,object> System.Linq.Enumerable.ToDictionary<int,int,object>(System.Collections.Generic.IEnumerable<int>,System.Func<int,int>,System.Func<int,object>)
		// System.Collections.Generic.Dictionary<int,object> System.Linq.Enumerable.ToDictionary<int,int,object>(System.Collections.Generic.IEnumerable<int>,System.Func<int,int>,System.Func<int,object>,System.Collections.Generic.IEqualityComparer<int>)
		// System.Collections.Generic.Dictionary<int,object> System.Linq.Enumerable.ToDictionary<object,int,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>,System.Func<object,object>)
		// System.Collections.Generic.Dictionary<int,object> System.Linq.Enumerable.ToDictionary<object,int,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int>,System.Func<object,object>,System.Collections.Generic.IEqualityComparer<int>)
		// System.Collections.Generic.Dictionary<object,object> System.Linq.Enumerable.ToDictionary<object,object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>,System.Func<object,object>,System.Collections.Generic.IEqualityComparer<object>)
		// System.Collections.Generic.Dictionary<object,object> System.Linq.Enumerable.ToDictionary<object,object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,object>)
		// System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int,object>> System.Linq.Enumerable.ToList<System.Collections.Generic.KeyValuePair<int,object>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>)
		// System.Collections.Generic.List<int> System.Linq.Enumerable.ToList<int>(System.Collections.Generic.IEnumerable<int>)
		// System.Collections.Generic.List<long> System.Linq.Enumerable.ToList<long>(System.Collections.Generic.IEnumerable<long>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<Loom.DelayedQueueItem> System.Linq.Enumerable.Where<Loom.DelayedQueueItem>(System.Collections.Generic.IEnumerable<Loom.DelayedQueueItem>,System.Func<Loom.DelayedQueueItem,bool>)
		// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,long>> System.Linq.Enumerable.Where<System.Collections.Generic.KeyValuePair<int,long>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,long>>,System.Func<System.Collections.Generic.KeyValuePair<int,long>,bool>)
		// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>> System.Linq.Enumerable.Where<System.Collections.Generic.KeyValuePair<int,object>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>,System.Func<System.Collections.Generic.KeyValuePair<int,object>,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.WhereIterator<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,int,bool>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.Zip<object,object,byte>(System.Collections.Generic.IEnumerable<object>,System.Collections.Generic.IEnumerable<object>,System.Func<object,object,byte>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.ZipIterator<object,object,byte>(System.Collections.Generic.IEnumerable<object>,System.Collections.Generic.IEnumerable<object>,System.Func<object,object,byte>)
		// System.Collections.Generic.IEnumerable<byte> System.Linq.Enumerable.Iterator<int>.Select<byte>(System.Func<int,byte>)
		// System.Collections.Generic.IEnumerable<double> System.Linq.Enumerable.Iterator<object>.Select<double>(System.Func<object,double>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<int,object>>.Select<int>(System.Func<System.Collections.Generic.KeyValuePair<int,object>,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<byte>.Select<int>(System.Func<byte,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<int>.Select<int>(System.Func<int,int>)
		// System.Collections.Generic.IEnumerable<int> System.Linq.Enumerable.Iterator<object>.Select<int>(System.Func<object,int>)
		// System.Collections.Generic.IEnumerable<long> System.Linq.Enumerable.Iterator<object>.Select<long>(System.Func<object,long>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<int,long>>.Select<object>(System.Func<System.Collections.Generic.KeyValuePair<int,long>,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<System.Collections.Generic.KeyValuePair<int,object>>.Select<object>(System.Func<System.Collections.Generic.KeyValuePair<int,object>,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<int>.Select<object>(System.Func<int,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<long>.Select<object>(System.Func<long,object>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Iterator<object>.Select<object>(System.Func<object,object>)
		// System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<int,long>> System.Linq.IOrderedEnumerable<System.Collections.Generic.KeyValuePair<int,long>>.CreateOrderedEnumerable<int>(System.Func<System.Collections.Generic.KeyValuePair<int,long>,int>,System.Collections.Generic.IComparer<int>,bool)
		// System.Linq.IOrderedEnumerable<int> System.Linq.IOrderedEnumerable<int>.CreateOrderedEnumerable<int>(System.Func<int,int>,System.Collections.Generic.IComparer<int>,bool)
		// int System.Runtime.CompilerServices.Unsafe.SizeOf<byte>()
		// int System.Runtime.CompilerServices.Unsafe.SizeOf<int>()
		// int System.Runtime.InteropServices.Marshal.SizeOf<object>(object)
		// System.Void System.Runtime.InteropServices.Marshal.StructureToPtr<object>(object,System.IntPtr,bool)
		// System.Span<int> System.Runtime.InteropServices.MemoryMarshal.Cast<byte,int>(System.Span<byte>)
		// bool System.SpanHelpers.IsReferenceOrContainsReferences<byte>()
		// bool System.SpanHelpers.IsReferenceOrContainsReferences<int>()
		// byte UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<byte>(System.IntPtr)
		// object UnityEngine.AndroidJNIHelper.ConvertFromJNIArray<object>(System.IntPtr)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetFieldID<object>(System.IntPtr,string,bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetMethodID<byte>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine.AndroidJNIHelper.GetMethodID<object>(System.IntPtr,string,object[],bool)
		// byte UnityEngine.AndroidJavaObject.Call<byte>(string,object[])
		// object UnityEngine.AndroidJavaObject.Call<object>(string,object[])
		// byte UnityEngine.AndroidJavaObject.CallStatic<byte>(string,object[])
		// object UnityEngine.AndroidJavaObject.CallStatic<object>(string,object[])
		// object UnityEngine.AndroidJavaObject.GetStatic<object>(string)
		// byte UnityEngine.AndroidJavaObject._Call<byte>(string,object[])
		// object UnityEngine.AndroidJavaObject._Call<object>(string,object[])
		// byte UnityEngine.AndroidJavaObject._CallStatic<byte>(string,object[])
		// object UnityEngine.AndroidJavaObject._CallStatic<object>(string,object[])
		// object UnityEngine.AndroidJavaObject._GetStatic<object>(string)
		// object UnityEngine.AssetBundle.LoadAsset<object>(string)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInChildren<object>()
		// object UnityEngine.Component.GetComponentInParent<object>()
		// object[] UnityEngine.Component.GetComponents<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.Component.TryGetComponent<object>(object&)
		// bool UnityEngine.EventSystems.ExecuteEvents.Execute<object>(UnityEngine.GameObject,UnityEngine.EventSystems.BaseEventData,UnityEngine.EventSystems.ExecuteEvents.EventFunction<object>)
		// UnityEngine.GameObject UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy<object>(UnityEngine.GameObject,UnityEngine.EventSystems.BaseEventData,UnityEngine.EventSystems.ExecuteEvents.EventFunction<object>)
		// System.Void UnityEngine.EventSystems.ExecuteEvents.GetEventList<object>(UnityEngine.GameObject,System.Collections.Generic.IList<UnityEngine.EventSystems.IEventSystemHandler>)
		// bool UnityEngine.EventSystems.ExecuteEvents.ShouldSendToComponent<object>(UnityEngine.Component)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponentInChildren<object>(bool)
		// object UnityEngine.GameObject.GetComponentInParent<object>()
		// object UnityEngine.GameObject.GetComponentInParent<object>(bool)
		// System.Void UnityEngine.GameObject.GetComponents<object>(System.Collections.Generic.List<object>)
		// object[] UnityEngine.GameObject.GetComponents<object>()
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>(bool)
		// bool UnityEngine.GameObject.TryGetComponent<object>(object&)
		// object UnityEngine.JsonUtility.FromJson<object>(string)
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object[] UnityEngine.Object.FindObjectsOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// object[] UnityEngine.Resources.ConvertObjects<object>(UnityEngine.Object[])
		// object[] UnityEngine.Resources.FindObjectsOfTypeAll<object>()
		// object UnityEngine.Resources.GetBuiltinResource<object>(string)
		// object UnityEngine.Resources.Load<object>(string)
		// object[] UnityEngine.Resources.LoadAll<object>(string)
		// UnityEngine.ResourceRequest UnityEngine.Resources.LoadAsync<object>(string)
		// byte UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<byte>(System.IntPtr)
		// object UnityEngine._AndroidJNIHelper.ConvertFromJNIArray<object>(System.IntPtr)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetFieldID<object>(System.IntPtr,string,bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetMethodID<byte>(System.IntPtr,string,object[],bool)
		// System.IntPtr UnityEngine._AndroidJNIHelper.GetMethodID<object>(System.IntPtr,string,object[],bool)
		// string UnityEngine._AndroidJNIHelper.GetSignature<byte>(object[])
		// string UnityEngine._AndroidJNIHelper.GetSignature<object>(object[])
		// string string.Join<int>(string,System.Collections.Generic.IEnumerable<int>)
	}
}