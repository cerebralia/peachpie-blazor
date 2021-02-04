﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Pchp.Core;
using Pchp.Core.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace BlazorApp2.Client.PHP
{
	[Route("/{Script}")]
	public class PHPRouter : ComponentBase
	{
		[Parameter] public string Script { get; set; }
		[Inject] NavigationManager NavManager { get; set; }

		private Context ctx { get; set; } = Context.CreateEmpty();
		private Context.ScriptInfo ExeScript { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder __builder)
		{
			EvalScript(__builder);
		}

		private void EvalScript(RenderTreeBuilder __builder)
		{
			using MemoryStream buffer = new MemoryStream();
			using StreamWriter writer = new StreamWriter(buffer, System.Text.Encoding.UTF8);
			ctx.Output = writer;
			ExeScript.Evaluate(ctx, ctx.Globals, null);

			writer.Flush();
			buffer.Position = 0;

			using StreamReader reader = new StreamReader(buffer, System.Text.Encoding.UTF8);
			__builder.AddMarkupContent(4, reader.ReadToEnd());
		}

		protected override void OnInitialized()
		{
			var uri = NavManager.ToAbsoluteUri(NavManager.Uri);

			if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("action", out var _action))
			{
				ctx.Get.Add("action", _action);
			}

			NavManager.LocationChanged += HandleLocationChanged;
		}

		//https://chrissainty.com/working-with-query-strings-in-blazor/
		void HandleLocationChanged(object sender, LocationChangedEventArgs e)
		{
			var uri = NavManager.ToAbsoluteUri(NavManager.Uri);

			if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("action", out var _action))
			{
				ctx.Get.Add("action", _action);
			}

			StateHasChanged();
		}

		protected override void OnParametersSet()
		{
			Console.WriteLine("OnParametersSet.");
			
			Assembly phpassembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == "ClassLibrary1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
			Context.AddScriptReference(phpassembly);

			Console.WriteLine("Script asssemblies references:");
			WriteAssemblies(Context.GetScriptReferences());

			Console.WriteLine("Current assemblies:");
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			WriteAssemblies(assemblies);

			Console.WriteLine($"Trying resolve script {Script}...");
			ExeScript = Context.TryGetDeclaredScript(Script);
			Console.WriteLine((ExeScript.IsValid) ? "Valid" : "Invalid");

			Console.WriteLine($"Script {Script}");
		}

		private void WriteAssemblies(IReadOnlyCollection<Assembly> assemblies)
		{
			Console.WriteLine($"Count: {assemblies.Count}");
			foreach (var assembly in assemblies)
			{
				Console.WriteLine(assembly);
			}
		}
	}
}
