using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;

namespace WPFStreamerAsync
{

    public class SubTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string l_result = value as String;

            if (l_result != null)
            {
                l_result = l_result.Replace("MFVideoFormat_", "");
                l_result = l_result.Replace("MFAudioFormat_", "");
            }

            return l_result;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class MediaTypeManager
    {

        private object mCurrentSource = null;

        public object CurrentSource
        {
            get => this.mCurrentSource;
            set
            {
                this.mCurrentSource = value;
                createGroupSubType(value);
            }
        }

        private object mCurrentSubType = null;

        public object CurrentSubType
        {
            get => this.mCurrentSubType;
            set
            {
                this.mCurrentSubType = value;
                createGroupMediaTypes(value);
            }
        }

        ObservableCollection<string> mSubTypesCollection = new ObservableCollection<string>();

        CollectionViewSource mSubTypesViewSource = new CollectionViewSource();

        public ICollectionView SubTypes { get => mSubTypesViewSource.View; }

        ObservableCollection<XmlNode> mMediaTypeCollection = new ObservableCollection<XmlNode>();

        CollectionViewSource mMediaTypesViewSource = new CollectionViewSource();

        public ICollectionView MediaTypes { get => mMediaTypesViewSource.View; }

        private void createGroupSubType(object aCurrentSource)
        {
            var lCurrentSourceNode = aCurrentSource as XmlNode;

            if (lCurrentSourceNode == null)
                return;

            var lSubTypesNode = lCurrentSourceNode.SelectNodes("PresentationDescriptor/StreamDescriptor/MediaTypes/MediaType/MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

            if (lSubTypesNode == null)
                return;

            mSubTypesCollection.Clear();

            foreach (XmlNode item in lSubTypesNode)
            {
                if (!mSubTypesCollection.Contains(item.Value))
                    mSubTypesCollection.Add(item.Value);
            }
        }

        private void createGroupMediaTypes(object aCurrentSubType)
        {
            var lCurrentSubType = aCurrentSubType as string;

            var lCurrentSourceNode = mCurrentSource as XmlNode;

            if (lCurrentSourceNode == null)
                return;

            var lMediaTypesNode = lCurrentSourceNode.SelectNodes("PresentationDescriptor/StreamDescriptor/MediaTypes/MediaType[MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue[@Value='" + lCurrentSubType + "']]");

            if (lMediaTypesNode == null)
                return;

            mMediaTypeCollection.Clear();

            foreach (XmlNode item in lMediaTypesNode)
            {
                mMediaTypeCollection.Add(item);
            }
        }

        public MediaTypeManager()
        {
            mMediaTypesViewSource.Source = mMediaTypeCollection;

            mSubTypesViewSource.Source = mSubTypesCollection;
        }
    }
}
