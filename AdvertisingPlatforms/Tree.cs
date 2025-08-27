using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvertisingPlatforms
{
    public class Tree // Дерево идеально подойдет для этой задачи (если имеется огромное количество регионов и огромное количество рекламных площадок). Так же этот подход идеально подходит для вывода всех подветвей дерева.
    {
        private Node root; 
        public Tree()
        {
            root = new Node(""); // Корень пустой (вдруг кроме /ru будут и другие регионы)
        }

        public List<string> Search(string location) => // Поиск рекламных площадок по заданному пути
            root.FindPlatforms(location) ?? [];

        public void Reload(IEnumerable<string> lines) // Предполагается, что загрузка производиться будет крайне редко, поэтому подобный грубый вариант будет оптимальным
        {
            var newRoot = new Node(""); // Корень пустой (вдруг кроме /ru будут и другие регионы)
            AddFromLines(newRoot, lines);
            root = newRoot; // Оставляем для сборщика мусора старый root
        }

        private class Node // Вершина дерева содержит в себе все рекламные платформы (их может быть несколько), локацию для размещения рекламы и список дочерних вершин (можно заменить на очередь для оптимизации).
        {
            private readonly List<string> advertisingPlatforms = [];
            private readonly string location = "";
            private readonly List<Node> children = [];
            private readonly Lock childrenLock = new();

            public Node(string location, string? advertisingPlatform = null)
            {
                this.location = location;
                if (!string.IsNullOrWhiteSpace(advertisingPlatform))
                    AddPlatform(advertisingPlatform);
            }

            public List<string>? FindPlatforms(string path)
            {
                ArgumentNullException.ThrowIfNull(path);

                var parts = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries); // Если вдруг путь содержит несколько подряд идущих путей
                var collected = new List<string>();

                if (FindAndCollect(parts, 0, collected))
                    return [.. collected.Distinct(StringComparer.OrdinalIgnoreCase)]; // Собираем все площадки на обратном пути (без повторяющихся элементов)

                return null; // Путь не найден
            }

            private bool FindAndCollect(string[] parts, int index, List<string> collected)
            {
                // Добавляем площадки текущего узла
                collected.AddRange(advertisingPlatforms);

                if (index == parts.Length) return true; // Дошли до искомой ноды

                var nextSegment = parts[index];
                var child = children.FirstOrDefault( // Поиск следующей локации
                    c => c.GetLocation().Equals(nextSegment, StringComparison.OrdinalIgnoreCase));

                if (child == null) return false; // Путь прервался — ноды нет
                return child.FindAndCollect(parts, index + 1, collected);
            }

            public string GetLocation() => location;
            public IReadOnlyList<Node> GetChildren() => children;
            public IReadOnlyList<string> GetPlatforms() => advertisingPlatforms;

            // Если ребёнок уже есть — вернём его, иначе создадим и добавим.
            public Node GetOrCreateChild(string segment)
            {
                ArgumentNullException.ThrowIfNull(segment);

                lock (childrenLock)
                {
                    var existing = children.FirstOrDefault( // Поиск существования ребенка
                        n => n.GetLocation().Equals(segment, StringComparison.OrdinalIgnoreCase));
                    if (existing != null) return existing;

                    var child = new Node(segment); // existing == null
                    children.Add(child);
                    return child;
                }
            }

            // Добавляем площадку в текущий узел (без дублей, нечувствительно к регистру)
            public void AddPlatform(string platform)
            {
                if (string.IsNullOrWhiteSpace(platform)) return;
                if (!advertisingPlatforms.Contains(platform, StringComparer.OrdinalIgnoreCase))
                    advertisingPlatforms.Add(platform);
            }
        }

        // Метод добавления нод в дерево согласно одной строки из ТЗ
        private static void AddFromString(Node root, string line) // Можно вынести в отдельный статичный класс, чтобы класс с деревом не содержал парсер файла, но тогда придется открыть класс Node
        {
            ArgumentNullException.ThrowIfNull(root);
            if (string.IsNullOrWhiteSpace(line)) return;

            var split = line.Split(':', 2);
            if (split.Length != 2) return;

            var platform = split[0].Trim();
            if (string.IsNullOrEmpty(platform)) return;

            var locations = split[1].Split(',', StringSplitOptions.RemoveEmptyEntries); // По запятым разбиваем локации
            foreach (var rawLoc in locations)
            {
                var loc = rawLoc.Trim();
                if (string.IsNullOrEmpty(loc)) continue; // Если пустая локация, то не будем ее учитывать и перейдем к следующей

                var segments = loc.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries); // Разбивка на подлокации
                var cur = root;
                foreach (var seg in segments)
                {
                    cur = cur.GetOrCreateChild(seg);
                }
                cur.AddPlatform(platform); // Платформу добавляем только в нижнюю локацию
            }
        }

        private static void AddFromLines(Node root, IEnumerable<string> lines) // Можно вынести в отдельный статичный класс, чтобы класс с деревом не содержал парсер файла, но тогда придется открыть класс Node
        {
            foreach (var l in lines) AddFromString(root, l);
        }
    }
}
