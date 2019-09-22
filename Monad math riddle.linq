<Query Kind="Program">
  <Output>DataGrids</Output>
</Query>

//
// Math riddle:
// Given 8 digits, 1..8, (a, b, c, d, e, f, g, h) divvy them up into three numbers as follows:
// *	Number1 = abc
// *	Number2 = def
// *	Number3 =  gh
// 
// Such that:
// 		  abc			For example:  234
// 		- def						- 156
//		-----						-----
// 		=  gh						=  78
// where in the example: (a,b,c,d,e,f,g,h)=(2,3,4,1,5,6,7,8)
//
// Constraints reducing the number of trial checks:
// 		In order to have a (3 digit number: abc) - (a 3 digit number: def)
//		resulting in a (2 digit number: gh)
//		*	a == d + 1 and
//		*	e > b
//
// I turned the brute force solution into a monadic solution in two steps:
// *	Take the nested foreach brute-force solution and turn them out into a nested SelectMany
// *	Take the nested SelectMany and turn them into a chained SelectMany solution.
//

class Program
{
	static void Main()
	{
		Console.WriteLine("Brute force");
		var rsBruteForce = BruteForce.BruteForceThroughForeach();
		WriteAll(rsBruteForce);

		Console.WriteLine();
		Console.WriteLine("Use Nested SelectMany");
		var rsNested = NestedSelectMany.UseNestedSelectMany();
		WriteAll(rsNested);

		Console.WriteLine();
		Console.WriteLine("Use chained selectMany");
		var rsChained = ChainedSelectMany.UseChainedSelectMany();
		WriteAll(rsChained);
	}
	
	static void WriteAll(IEnumerable<string> texts)
	{
		int cnt = 0;
		texts.ToList().ForEach(t => {int i = ++cnt; Console.WriteLine($"{i,2}.  {t}"); });
	}
}

static class Riddle
{
	public static int dig2Num(int u, int v, int w) => 100 * u + 10 * v + w;
	public static int dig2Num(int v, int w) => 10 * v + w;

	public static bool Check(int x, int y, int z) => x - y == z;
	public static string ToString(int x, int y, int z) => $"{x} - {y} = {z}";
}

class BruteForce
{
	//
	// Brute Force method
	//
	public static IEnumerable<string> BruteForceThroughForeach()
	{
		var res = new List<string>();
		foreach (var a in Enumerable.Range(2, 7))
		{
			var d = a - 1;

			foreach (var b in Enumerable.Range(1, 8).Where(n => new[] { a, d }.All(m => m != n)))
			{
				foreach (var c in Enumerable.Range(1, 8).Where(n => new[] { a, b, d }.All(m => m != n)))
				{
					var x = Riddle.dig2Num(a, b, c);
					foreach (var e in Enumerable.Range(1, 8).Where(n => new[] { a, b, c, d }.All(m => m != n) && n > b))
					{
						foreach (var f in Enumerable.Range(1, 8).Where(n => new[] { a, b, c, d, e }.All(m => m != n)))
						{
							var y = Riddle.dig2Num(d, e, f);
							foreach (var g in Enumerable.Range(1, 8).Where(n => new[] { a, b, c, d, e, f }.All(m => m != n)))
							{
								foreach (var h in Enumerable.Range(1, 8).Where(n => new[] { a, b, c, d, e, f, g }.All(m => m != n)))
								{
									var z = Riddle.dig2Num(g, h);
									if (Riddle.Check(x, y, z)) res.Add(Riddle.ToString(x, y, z));
								}
							}
						}
					}
				}
			}
		}

		return res;
	}
}

class NestedSelectMany
{
	public static IEnumerable<string> UseNestedSelectMany()
	{
		var m = Enumerable.Range(2, 7);
		var rs = m.SelectMany(a => bFunc((a, a - 1)));
		return rs;
	}

	private static IEnumerable<string> bFunc((int a, int d) arg)
	{
		var m = Enumerable.Range(1, 8).Where(n => new[] { arg.a, arg.d }.All(k => k != n));
		var rs = m.SelectMany(b => cFunc((arg.a, b, arg.d)));
		return rs;
	}

	private static IEnumerable<string> cFunc((int a, int b, int d) arg)
	{
		var m = Enumerable.Range(1, 8).Where(n => new[] { arg.a, arg.b, arg.d }.All(k => k != n));
		var rs = m.SelectMany(c => eFunc((arg.a, arg.b, c, arg.d, Riddle.dig2Num(arg.a, arg.b, c))));
		return rs;
	}

	private static IEnumerable<string> eFunc((int a, int b, int c, int d, int x) arg)
	{
		var m = Enumerable.Range(1, 8).Where(n => new[] { arg.a, arg.b, arg.c, arg.d }.All(k => k != n) && n > arg.b);
		var rs = m.SelectMany(e => fFunc((arg.a, arg.b, arg.c, arg.d, e, arg.x)));
		return rs;
	}

	private static IEnumerable<string> fFunc((int a, int b, int c, int d, int e, int x) arg)
	{
		var m = Enumerable.Range(1, 8).Where(n => new[] { arg.a, arg.b, arg.c, arg.d, arg.e }.All(k => k != n));
		var rs = m.SelectMany(f => gFunc((arg.a, arg.b, arg.c, arg.d, arg.e, f, arg.x, Riddle.dig2Num(arg.d, arg.e, f))));
		return rs;
	}

	private static IEnumerable<string> gFunc((int a, int b, int c, int d, int e, int f, int x, int y) arg)
	{
		var m = Enumerable.Range(1, 8).Where(n => new[] { arg.a, arg.b, arg.c, arg.d, arg.e, arg.f }.All(k => k != n));
		var rs = m.SelectMany(g => hFunc((arg.a, arg.b, arg.c, arg.d, arg.e, arg.f, g, arg.x, arg.y)));
		return rs;
	}

	private static IEnumerable<string> hFunc((int a, int b, int c, int d, int e, int f, int g, int x, int y) arg)
	{
		var m = Enumerable.Range(1, 8).Where(n => new[] { arg.a, arg.b, arg.c, arg.d, arg.e, arg.f, arg.g }.All(k => k != n))
			.Where(h => Riddle.Check(arg.x, arg.y, Riddle.dig2Num(arg.g, h)));

		// This, of-course, could be a Select(..) like so:
		// 		var rs = m.Select(h => $"{arg.x} - {arg.y} = {dig2Num(arg.g, h)}");
		// without the need to call resFunc(..), which is shorter.
		// But I wanted to demonstrate the SelectMany(..)
		var rs = m.SelectMany(h => resFunc((arg.x, arg.y, Riddle.dig2Num(arg.g, h))));
		return rs;
	}

	private static IEnumerable<string> resFunc((int x, int y, int z) arg)
	{
		yield return Riddle.ToString(arg.x, arg.y, arg.z);
	}
}

class ChainedSelectMany
{
	public static IEnumerable<string> UseChainedSelectMany()
	{
		// Using associative or chain rule
		// IEnumerable<R> SelectMany(this IEnumerable<T> m, Func<T, IEnumerable<R>> g ° f)
		// IEnumerable<R> m.SelectMany(g ° f) = m.Select(f).Select(g)
		var rs = Enumerable.Range(2, 7)     // a: ranges 2..8 we start at 2 in order to satisfy constraint: d = a - 1

			.SelectMany(a => Enumerable.Range(1, 8)                 // Cross product of a x b with
				.Where(b => new[] { a, a - 1 }.All(n => n != b))	// Constraints: b != a, d
				.Select(b => (a, b, a - 1)))                        // make a single tuple for the next cross produt

			.SelectMany(((int a, int b, int d) arg) => Enumerable.Range(1, 8)				// Cross product a x b x c with 
				.Where(c => new[] { arg.a, arg.b, arg.d }.All(n => n != c))					// Constraints: c != a, b, d
				.Select(c => (arg.a, arg.b, c, arg.d, Riddle.dig2Num(arg.a, arg.b, c))))	// Set a single tuple variable for next xross product

			.SelectMany(((int a, int b, int c, int d, int x) arg) => Enumerable.Range(1, 8) 	// Cross product a x b x c x e with
				.Where(e => new[] { arg.a, arg.b, arg.c, arg.d }.All(n => n != e) && e > arg.b)	// Constraints: e != a, b, c, d and e > b
				.Select(e => (arg.a, arg.b, arg.c, arg.d, e, arg.x)))							// Set a single tuple variable for next xross product

			.SelectMany(((int a, int b, int c, int d, int e, int x) arg) => Enumerable.Range(1, 8)				// Cross product a x b x c x e x f with
				.Where(f => new[] { arg.a, arg.b, arg.c, arg.d, arg.e }.All(n => n != f))						// Constraints: f != a, b, c, d, e
				.Select(f => (arg.a, arg.b, arg.c, arg.d, arg.e, f, arg.x, Riddle.dig2Num(arg.d, arg.e, f))))	// Set a single tuple variable for next xross product

			.SelectMany(((int a, int b, int c, int d, int e, int f, int x, int y) arg) => Enumerable.Range(1, 8)    // Cross product a x b x c x e x f x g with
				.Where(g => new[] { arg.a, arg.b, arg.c, arg.d, arg.e, arg.f }.All(n => n != g))                    // Constraints: g != a, b, c, d, e, f
				.Select(g => (arg.a, arg.b, arg.c, arg.d, arg.e, arg.f, g, arg.x, arg.y)))                          // Set a single tuple variable for next xross product

			.SelectMany(((int a, int b, int c, int d, int e, int f, int g, int x, int y) arg) => Enumerable.Range(1, 8) // Cross product a x b x c x e x f x g x h with
				.Where(h => new[] { arg.a, arg.b, arg.c, arg.d, arg.e, arg.f, arg.g }.All(n => n != h))                 // Constraints: h != a, b, c, d, e, f, g
				.Where(h => Riddle.Check(arg.x, arg.y, Riddle.dig2Num(arg.g, h)))                                   	// Constraints: x - y == z
				.Select(h => (arg.x, arg.y, Riddle.dig2Num(arg.g, h))))                                                 // Set a single tuple variable for next xross product

			// Remark:
			// Of-course this step of SelectMany is superfluous, the previous SelectMany(..) could have ended with
			//		Select(h => $"{arg.x} - {arg.y} = {dig2Num(arg.g, h)}");
			// 		instead of
			//			Select(h => (arg.x, arg.y, dig2Num(arg.g, h)))
			//		and end it there.
			.SelectMany(((int x, int y, int z) arg) => new List<string> { Riddle.ToString(arg.x, arg.y, arg.z) });

		return rs;
	}
}

//