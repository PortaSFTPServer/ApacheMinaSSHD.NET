// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using java.lang;
using java.nio.file;
using java.util;
using java.util.function;

namespace ApacheMinaSSHD.NET.Wrapper.FileSystem
{
    internal class FilteredDirectoryStream : DirectoryStream
    {
        private readonly DirectoryStream _originalStream;
        private readonly Iterator _originalIterator;
        private readonly System.Func<java.nio.file.Path, bool> _includePath;
        private java.nio.file.Path? _nextFilteredPath;

        public FilteredDirectoryStream(
            DirectoryStream originalStream,
            System.Func<java.nio.file.Path, bool>? includePath = null)
        {
            _originalStream = originalStream;
            _originalIterator = originalStream.iterator();
            _includePath = includePath ?? (_ => true);
        }

        public static FilteredDirectoryStream HideExtensions(DirectoryStream stream, params string[] extensions)
        {
            var exts = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
            return new FilteredDirectoryStream(stream, path =>
            {
                string? name = path.getFileName()?.toString();
                if (string.IsNullOrWhiteSpace(name)) return true;
                int dot = name.LastIndexOf('.');
                if (dot < 0) return true;
                return !exts.Contains(name.Substring(dot));
            });
        }

        public static FilteredDirectoryStream ShowOnlyExtensions(DirectoryStream stream, params string[] extensions)
        {
            var exts = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
            return new FilteredDirectoryStream(stream, path =>
            {
                string? name = path.getFileName()?.toString();
                if (string.IsNullOrWhiteSpace(name)) return false;
                int dot = name.LastIndexOf('.');
                if (dot < 0) return false;
                return exts.Contains(name.Substring(dot));
            });
        }

        public static FilteredDirectoryStream HideNames(DirectoryStream stream, params string[] names)
        {
            var set = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            return new FilteredDirectoryStream(stream, path =>
            {
                string? name = path.getFileName()?.toString();
                return !string.IsNullOrWhiteSpace(name) && !set.Contains(name);
            });
        }

        public static FilteredDirectoryStream ShowOnlyNames(DirectoryStream stream, params string[] names)
        {
            var set = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            return new FilteredDirectoryStream(stream, path =>
            {
                string? name = path.getFileName()?.toString();
                return name != null && set.Contains(name);
            });
        }

        public static FilteredDirectoryStream HideDirectories(DirectoryStream stream)
        {
            return new FilteredDirectoryStream(stream, path =>
            {
                string? dir = path.toAbsolutePath()?.toString();
                return string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir);
            });
        }

        public static FilteredDirectoryStream ShowOnlyDirectories(DirectoryStream stream)
        {
            return new FilteredDirectoryStream(stream, path =>
            {
                string? dir = path.toAbsolutePath()?.toString();
                return !string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir);
            });
        }

        public void close()
        {
            _originalStream.close();
        }

        public void Dispose()
        {
            close();
        }

        public void forEach(Consumer action)
        {
            Iterator currentIterator = iterator();
            while (currentIterator.hasNext())
            {
                action.accept((java.nio.file.Path)currentIterator.next());
            }
        }

        public Iterator iterator()
        {
            return new FilteredPathIterator(this);
        }

        public Spliterator spliterator()
        {
            return Spliterators.spliteratorUnknownSize(iterator(), 0);
        }

        private bool ShouldInclude(java.nio.file.Path path)
        {
            return _includePath(path);
        }

        private class FilteredPathIterator : Iterator
        {
            private readonly FilteredDirectoryStream _parent;

            public FilteredPathIterator(FilteredDirectoryStream parent)
            {
                _parent = parent;
                FindNextFilteredPath();
            }

            private void FindNextFilteredPath()
            {
                _parent._nextFilteredPath = null;

                while (_parent._originalIterator.hasNext())
                {
                    java.nio.file.Path currentPath = (java.nio.file.Path)_parent._originalIterator.next();
                    if (_parent.ShouldInclude(currentPath))
                    {
                        _parent._nextFilteredPath = currentPath;
                        break;
                    }
                }
            }

            public bool hasNext()
            {
                return _parent._nextFilteredPath != null;
            }

            public object next()
            {
                if (_parent._nextFilteredPath == null)
                {
                    throw new NoSuchElementException();
                }

                java.nio.file.Path result = _parent._nextFilteredPath;
                FindNextFilteredPath();
                return result;
            }

            public void remove()
            {
                throw new UnsupportedOperationException("remove is not supported by this iterator.");
            }

            public void forEachRemaining(Consumer action)
            {
                if (_parent._nextFilteredPath != null)
                {
                    action.accept(_parent._nextFilteredPath);
                    _parent._nextFilteredPath = null;
                }

                while (_parent._originalIterator.hasNext())
                {
                    java.nio.file.Path currentPath = (java.nio.file.Path)_parent._originalIterator.next();
                    if (_parent.ShouldInclude(currentPath))
                    {
                        action.accept(currentPath);
                    }
                }
            }
        }
    }
}
