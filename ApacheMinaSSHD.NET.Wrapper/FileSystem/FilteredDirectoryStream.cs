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
            _includePath = includePath ?? (path => !IsHidden(path));
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

        public static bool IsHidden(java.nio.file.Path path)
        {
            if (path == null)
            {
                return false;
            }

            string fileName = path.getFileName()?.toString()!;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            if (fileName.StartsWith(".") && fileName != "." && fileName != "..")
            {
                return true;
            }

            return fileName.Contains("secret_data", StringComparison.OrdinalIgnoreCase);
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
