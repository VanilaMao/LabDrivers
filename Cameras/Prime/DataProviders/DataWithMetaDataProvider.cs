using System;
using System.Runtime.InteropServices;
using LabDrivers.Cameras.Prime.DllImports;

namespace LabDrivers.Cameras.Prime.DataProviders
{
    public abstract class DataWithMetaDataProvider : DataWithoutMetaDataProvider
    {
        // ReSharper disable once InconsistentNaming
        protected PvTypes.MD_Frame _mdFrame;

        public PvTypes.MD_Frame MdFrame
        {
            get => _mdFrame;
            private set => _mdFrame = value;
        }

        public PvTypes.MD_Frame_Header MdFrameHeader { get; private set; }

        public FrmMetadata FrameMetadata { get; private set; }
        public RoiMetadata[] RoiMetadatas { get; private set; }

        public int RoiCount { get; }

        public PvTypes.Md_Frame_Roi[] FrameRois { get; private set; }

        protected DataWithMetaDataProvider(DataProviderWithMetaDataContext context) : base(context)
        {
            RoiCount = context.RoiCount;
        }

        public override short[] TransformData()
        {

            IntPtr ptrMdFrame = Marshal.AllocHGlobal(Marshal.SizeOf(MdFrame));
            //get pointer to the structure
            Marshal.StructureToPtr(MdFrame, ptrMdFrame, false);

            if (Pvcam.pl_md_frame_decode(ptrMdFrame, SourceStream, FrameSize))
            {

                MdFrame = (PvTypes.MD_Frame) Marshal.PtrToStructure(ptrMdFrame, typeof(PvTypes.MD_Frame));
                ExtractFrameHeader();

                //Define ROI arrays based on number of regions
                FrameRois = new PvTypes.Md_Frame_Roi[RoiCount];

                //Initialize array 
                RoiMetadatas = new RoiMetadata[RoiCount];


                IntPtr ptr;
                int roiMdSize = Marshal.SizeOf(FrameRois[0]);
                for (Int16 i = 0; i < RoiCount; i++)
                {
                    //First roi metadata 
                    if (i == 0)
                    {
                        FrameRois[0] =
                            (PvTypes.Md_Frame_Roi) Marshal.PtrToStructure(MdFrame.roiArray,
                                typeof(PvTypes.Md_Frame_Roi));
                        // roi_md_size = Marshal.SizeOf(m_frame_roi[0]);
                    }
                    else
                    {

                        //subsequent ROI metadata at offset from the first roi pointer
                        if (IntPtr.Size == sizeof(Int64))
                        {
                            ptr = new IntPtr((MdFrame.roiArray).ToInt64() + roiMdSize * i);
                        }
                        else
                        {
                            ptr = new IntPtr((MdFrame.roiArray).ToInt32() + roiMdSize * i);

                        }

                        FrameRois[i] = (PvTypes.Md_Frame_Roi) Marshal.PtrToStructure(ptr, typeof(PvTypes.Md_Frame_Roi));
                    }

                    //Frame Roi Header can be extracted from above if required.
                    ExtractRoiHeader(FrameRois[i].header, i);

                }

                //free umnamaged pointers
                Marshal.FreeHGlobal(ptrMdFrame);
                ptrMdFrame = IntPtr.Zero;
                ptr = IntPtr.Zero;

                return CopyFromRioFrameToBuffer();
            }

            return null;
        }

        protected abstract short[] CopyFromRioFrameToBuffer();


        private void ExtractFrameHeader()
        {

            MdFrameHeader = (PvTypes.MD_Frame_Header)Marshal.PtrToStructure(MdFrame.header, typeof(PvTypes.MD_Frame_Header));

            FrameMetadata = new FrmMetadata
            {
                FrameNr = MdFrameHeader.frameNr,
                RoiCount = MdFrameHeader.roiCount,
                TimeStampBof = (MdFrameHeader.timeStampBOF) * (MdFrameHeader.timeStampResNs),
                TimeStampEof = (MdFrameHeader.timeStampEOF) * (MdFrameHeader.timeStampResNs),
                ExpTime = (MdFrameHeader.exposureTime) * (MdFrameHeader.exposureTimeResNs)
            };
            //populate important meta data

        }
        //Extract frame data from the MD structure and 
        private void ExtractRoiHeader(IntPtr ptrRoiHeader, Int16 roiNr)
        {

            PvTypes.MD_Frame_ROI_Header roiHeader =
                (PvTypes.MD_Frame_ROI_Header) Marshal.PtrToStructure(ptrRoiHeader,
                    typeof(PvTypes.MD_Frame_ROI_Header));

            //Initialize each object of array before assigning values
            RoiMetadatas[roiNr] = new RoiMetadata
            {
                //populate important meta data
                RoiNr = roiHeader.roiNr,
                S1 = roiHeader.roi.s1,
                S2 = roiHeader.roi.s2,
                P1 = roiHeader.roi.p1,
                P2 = roiHeader.roi.p2,
                TimeStampBor = (roiHeader.timestampBOR) * (MdFrameHeader.roiTimestampResN),
                TimeStampEor = (roiHeader.timestampEOR) * (MdFrameHeader.roiTimestampResN)
            };

        }
        public class RoiMetadata
        {
            public UInt32 RoiNr { get; set; }
            public UInt16 S1 { get; set; }
            public UInt16 S2 { get; set; }
            public UInt16 P1 { get; set; }
            public UInt16 P2 { get; set; }
            public UInt32 TimeStampBor { get; set; }
            public UInt32 TimeStampEor { get; set; }
        }
        public class FrmMetadata
        {
            public UInt32 FrameNr { get; set; }

            public UInt16 RoiCount { get; set; }

            public UInt32 TimeStampBof { get; set; }

            public UInt32 TimeStampEof { get; set; }

            public UInt32 ExpTime { get; set; }

        }
    }
}