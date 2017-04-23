﻿// Copyright (c) 2017 Emilian Cioca
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AssetManager
{
	public partial class formAssetManager : Form
	{
		public formAssetManager()
		{
			InitializeComponent();

			treeViewAssets.AfterSelect += (sender, e) => RefreshItemView();
			listBoxAssets.MouseDoubleClick += (sender, e) => OpenAsset();
			treeViewAssets.AfterSelect += (sender, e) => buttonAssetOpen.Enabled = true;
			encoderForm.FormClosing += (s, arg) => LoadEncoders();

			// Populate the workspace for the first time.
			RefreshWorkspace();

			// Watch the directory for changes and auto-refresh when needed.
			watcher = new FileSystemWatcher(Directory.GetCurrentDirectory());
			watcher.IncludeSubdirectories = true;
			watcher.EnableRaisingEvents = true;
			watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
			watcher.Changed += (sender, e) => RefreshWorkspaceDispatch();
			watcher.Created += (sender, e) => RefreshWorkspaceDispatch();
			watcher.Deleted += (sender, e) => RefreshWorkspaceDispatch();
		}

		//- Opens the selected asset with the default associated program.
		void OpenAsset()
		{
			var selection = listBoxAssets.SelectedItem;
			if (selection == null)
				return;

			var node = treeViewAssets.SelectedNode;
			if (node == null)
				return;

			var file = string.Join("\\", GetNodePath(node)) + "\\" + listBoxAssets.GetItemText(selection);

			try
			{
				System.Diagnostics.Process.Start(file);
			}
			catch (System.ComponentModel.Win32Exception)
			{}
		}

		//- Starts the packing process. Rebuilds the target Asset directory from scratch.
		private void Button_Pack_Click(object sender, EventArgs e)
		{
			Output.AppendLine("--- Started ---", Color.Azure);

			buttonClear.Enabled = false;
			ButtonPack.Enabled = false;
			ButtonUpdate.Enabled = false;

			LoadEncoders();
			var assetEncoders = encoderForm.GetEncoders();

			string inputRoot = Directory.GetCurrentDirectory();
			string commonRoot = inputRoot.Remove(inputRoot.LastIndexOf(Path.DirectorySeparatorChar));
			string outputRoot = $"{commonRoot}{Path.DirectorySeparatorChar}Assets";

			if (Directory.Exists(outputRoot))
				Directory.Delete(outputRoot, true);

			// Find all files in the workspace.
			// Convertible files are put through the appropriate binary converter.
			// Other files are copied as-is.
			var filesToCopy = new List<string>();
			var filesToConvert = new List<string>();
			foreach (var dir in Directory.GetDirectories(Directory.GetCurrentDirectory(), "*", SearchOption.TopDirectoryOnly))
			{
				foreach (string file in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
				{
					if (file.EndsWith(".meta", StringComparison.InvariantCultureIgnoreCase))
						continue;
				
					if (assetEncoders.Any(x => file.EndsWith(x.Key, StringComparison.InvariantCultureIgnoreCase)))
						filesToConvert.Add(file);
					else
						filesToCopy.Add(file);
				}
			}

			Directory.CreateDirectory(outputRoot);

			try
			{
				foreach (string file in filesToConvert)
				{
					Output.AppendLine($"Encoding {Path.GetFileName(file)}", Color.Azure);

					// Conversion should target the asset directory.
					// ...
				}

				foreach (string file in filesToCopy)
				{
					Output.AppendLine($"Copying {Path.GetFileName(file)}", Color.Azure);

					var outFile = file.Replace(inputRoot, outputRoot);
					if (!Directory.Exists(Path.GetDirectoryName(outFile)))
						Directory.CreateDirectory(Path.GetDirectoryName(outFile));

					File.Copy(file, outFile);
				}

				Output.AppendLine($"Done.", Color.Azure);
			}
			catch (IOException exception)
			{
				Output.AppendLine($"Error: ( {exception.Message} )", Color.Red);
			}

			buttonClear.Enabled = true;
			ButtonPack.Enabled = true;
			ButtonUpdate.Enabled = true;
		}

		void RefreshItemView()
		{
			listBoxAssets.Items.Clear();

			var node = treeViewAssets.SelectedNode;
			if (node == null)
				return;

			foreach (string file in Directory.GetFiles(string.Join("\\", GetNodePath(node))))
			{
				listBoxAssets.Items.Add(Path.GetFileName(file));
			}
		}

		void RefreshWorkspace()
		{
			var previousSelection = GetNodePath(treeViewAssets.SelectedNode);
			treeViewAssets.Nodes.Clear();

			ParseDirectory(Directory.GetCurrentDirectory(), null);

			// Reselect the old folder, if it still exists.
			if (previousSelection.Count != 0)
			{
				var node = treeViewAssets.Nodes.Find(previousSelection[0], false).FirstOrDefault();

				for (int i = 1; i < previousSelection.Count; ++i)
				{
					if (node == null)
						break;

					node = node.Nodes.Find(previousSelection[i], false).FirstOrDefault();
				}

				treeViewAssets.SelectedNode = node;
			}

			treeViewAssets.ExpandAll();
			RefreshItemView();

			// Find any files that don't yet have a .meta and initialize them.
			//...
		}

		delegate void RefreshWorkspaceDelegate();
		void RefreshWorkspaceDispatch()
		{
			// Thread-safe dispatch.
			if (treeViewAssets.InvokeRequired)
			{
				Invoke(new RefreshWorkspaceDelegate(RefreshWorkspace));
			}
			else
			{
				RefreshWorkspace();
			}
		}

		void ParseDirectory(string rootFolder, TreeNode root)
		{
			var directories = Directory.GetDirectories(rootFolder);
			Array.Sort(directories);

			foreach (string folder in directories)
			{
				// Add the file count to the name.
				var fileCount = Directory.GetFiles(folder).Count();
				string name = folder.Split('\\').Last();
				string text = fileCount == 0 ? name : name + " [" + fileCount.ToString() + "]";

				// Append the new node.
				var nodes = root?.Nodes ?? treeViewAssets.Nodes;
				var node = nodes.Add(name, text);

				// Recurse.
				ParseDirectory(folder, node);
			}
		}

		//- Build the full folder path from the folder tree.
		List<string> GetNodePath(TreeNode node)
		{
			var path = new List<string>();
			while (node != null)
			{
				path.Add(node.Name);
				node = node.Parent;
			}
			path.Reverse();

			return path;
		}

		private void LoadEncoders()
		{
			encoders.Clear();

			try
			{
				foreach (var dll in encoderForm.GetEncoders())
					encoders.Add(new Encoder(dll.Value));
			}
			catch (ArgumentException e)
			{
				Output.AppendLine(e.Message, Color.Red);
				encoders.Clear();
			}
		}

		//- Reads output from a native code converter.
		delegate void NativeLogDelegate(string text);
		private void NativeLog(string text)
		{
			if (text == null)
				return;

			if (Output.InvokeRequired)
			{
				Output.Invoke(new NativeLogDelegate(NativeLog), new object[] { text });
			}
			else
			{
				// Parse flags to set the color
				if (text.StartsWith("[e]", StringComparison.InvariantCultureIgnoreCase))
				{
					text = text.Substring(3);
					Output.SelectionColor = Color.Red;
				}
				else if (text.StartsWith("[w]", StringComparison.InvariantCultureIgnoreCase))
				{
					text = text.Substring(3);
					Output.SelectionColor = Color.Yellow;
				}
				else if (text.StartsWith("[s]", StringComparison.InvariantCultureIgnoreCase))
				{
					text = text.Substring(3);
					Output.SelectionColor = Color.LightGreen;
				}
				else if (text.StartsWith("[r]", StringComparison.InvariantCultureIgnoreCase))
				{
					text = text.Substring(3);
					Output.SelectionColor = Color.LightGray;
				}

				RichTextBoxExtensions.AppendText(Output, text + Environment.NewLine);
			}
		}

		// Automatically refresh the asset directory if there are any changes.
		FileSystemWatcher watcher;
		// The form responsible for editing the list of encoders.
		EncoderList encoderForm = new EncoderList();
		// The actual native-code encoders.
		List<Encoder> encoders = new List<Encoder>();

		private void buttonSettings_Click(object sender, EventArgs e)
		{
			encoderForm.Show();
			encoderForm.BringToFront();
		}

		private void ButtonUpdate_Click(object sender, EventArgs e)
		{
			LoadEncoders();


		}

		//- Browses to the root workspace directory.
		private void buttonWorkspaceOpen_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("explorer.exe", Directory.GetCurrentDirectory());
		}

		//- Browses to the selected folder directory.
		private void buttonAssetOpen_Click(object sender, EventArgs e)
		{
			var path = GetNodePath(treeViewAssets.SelectedNode);

			if (path.Count != 0)
				System.Diagnostics.Process.Start("explorer.exe", string.Join("\\", path));
		}

		//- Clears the output window.
		private void buttonClear_Click(object sender, EventArgs e)
		{
			Output.Clear();
		}
	}

	public static class RichTextBoxExtensions
	{
		delegate void SetOutputColorDelegate(RichTextBox box, Color color);
		public static void SetOutputColor(RichTextBox box, Color color)
		{
			if (box.InvokeRequired)
			{
				box.Invoke(new SetOutputColorDelegate(SetOutputColor), new object[] { box, color });
			}
			else
			{
				box.ForeColor = color;
			}
		}

		delegate void AppendTextDelegate(RichTextBox box, string value, Color color = default(Color));
		public static void AppendText(this RichTextBox box, string value, Color color = default(Color))
		{
			if (box.InvokeRequired)
			{
				box.Invoke(new AppendTextDelegate(AppendText), new object[] { box, value, color });
			}
			else
			{
				// Only change the box color if an explicit one was given.
				if (color != default(Color))
					box.SelectionColor = color;

				box.SelectionStart = box.TextLength;
				box.SelectionLength = 0;

				box.AppendText(value);
				box.SelectionColor = box.ForeColor;

				box.ScrollToCaret();
			}
		}

		delegate void AppendLineDelegate(RichTextBox box, string value, Color color = default(Color));
		public static void AppendLine(this RichTextBox box, string value, Color color = default(Color))
		{
			if (box.InvokeRequired)
			{
				box.Invoke(new AppendLineDelegate(AppendLine), new object[] { box, value, color });
			}
			else
			{
				if (box.Text.Length != 0)
					value = "\r\n" + value;

				AppendText(box, value, color);
			}
		}
	}
}
