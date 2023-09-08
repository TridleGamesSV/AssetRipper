using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.GUI.Electron.Pages.Assets
{
	public class ViewModel : PageModel
	{
		private readonly ILogger<ViewModel> _logger;
		public IUnityObjectBase Asset { get; private set; } = default!;

		public string ImageSource
		{
			get
			{
				DirectBitmap bitmap = Bitmap;
				if (bitmap != default)
				{
					MemoryStream stream = new();
					bitmap.SaveAsPng(stream);
					return $"data:image/png;base64,{Convert.ToBase64String(stream.ToArray(), Base64FormattingOptions.None)}";
				}
				return "";
			}
		}

		private DirectBitmap Bitmap
		{
			get
			{
				switch (Asset)
				{
					case ITexture2D texture:
						{
							if (TextureConverter.TryConvertToBitmap(texture, out DirectBitmap bitmap))
							{
								return bitmap;
							}
						}
						goto default;
					case ITerrainData terrainData:
						return TerrainHeatmapExporter.GetBitmap(terrainData);
					default:
						return default;
				}
			}
		}

		public string Text
		{
			get
			{
				return Asset switch
				{
					ITextAsset textAsset => textAsset.Script_C49,
					_ => "",
				};
			}
		}

		public IEnumerable<(string, IUnityObjectBase)> CustomReferenceProperties
		{
			get
			{
				List<(string, IUnityObjectBase)> list = new();
				if (Asset.MainAsset is not null && Asset.MainAsset != Asset)
				{
					list.Add(("Main Asset", Asset.MainAsset));
				}
				switch (Asset)
				{
					case IComponent component:
						{
							if (component.GameObject_C2P is { } gameObject)
							{
								list.Add(("GameObject", gameObject));
							}
							if (component is IMonoBehaviour monoBehaviour && monoBehaviour.Script_C114P is { } monoScript)
							{
								list.Add(("Script", monoScript));
							}
							else if (component is IMeshFilter meshFilter && meshFilter.Mesh_C33P is { } mesh)
							{
								list.Add(("Mesh", mesh));
							}
						}
						break;
					case IGameObject gameObject:
						{
							if (gameObject.TryGetComponent(out ITransform? transform))
							{
								list.Add(("Transform", transform));
							}
						}
						break;
					case IMaterial material:
						{
							if (material.Shader_C21P is { } shader)
							{
								list.Add(("Shader", shader));
							}
						}
						break;
					case SpriteInformationObject spriteInformationObject:
						{
							list.Add(("Texture", spriteInformationObject.Texture));
						}
						break;
					default:
						break;
				}
				return list.Count != 0 ? list : Enumerable.Empty<(string, IUnityObjectBase)>();
			}
		}

		public IEnumerable<(string, string)> CustomStringProperties
		{
			get
			{
				switch (Asset)
				{
					case IMonoScript monoScript:
						{
							return ToPropertyArray(monoScript);
						}
					case IMonoBehaviour monoBehaviour:
						{
							if (monoBehaviour.Script_C114P is { } monoScript)
							{
								return ToPropertyArray(monoScript);
							}
							else
							{
								goto default;
							}
						}
					case ITexture2D texture2D:
						{
							return new (string, string)[]
							{
								("Width", texture2D.Width_C28.ToString()),
								("Height", texture2D.Height_C28.ToString()),
								("Format", texture2D.Format_C28E.ToString()),
							};
						}
					case ITerrainData terrainData:
						{
							return new (string, string)[]
							{
								("Width", terrainData.Heightmap_C156.GetWidth().ToString()),
								("Height", terrainData.Heightmap_C156.GetHeight().ToString()),
							};
						}
					default:
						return Enumerable.Empty<(string, string)>();
				}

				static (string, string)[] ToPropertyArray(IMonoScript monoScript)
				{
					return new (string, string)[]
					{
						("Assembly Name", monoScript.AssemblyName_C115),
						("Namespace", monoScript.Namespace_C115),
						("Class Name", monoScript.ClassName_C115),
					};
				}
			}
		}

		public ViewModel(ILogger<ViewModel> logger)
		{
			_logger = logger;
		}

		public IActionResult OnGet(string? path)
		{
			if (string.IsNullOrEmpty(path))
			{
				_logger.LogError("Path is null");
				return Redirect("/");
			}
			else if (Program.Ripper.IsLoaded && Program.Ripper.GameStructure.FileCollection.TryGetAsset(AssetPath.FromJson(path), out IUnityObjectBase? asset))
			{
				Asset = asset;
				return Page();
			}
			else
			{
				return NotFound();
			}
		}
	}
}