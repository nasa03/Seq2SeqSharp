﻿//	Copyright (c) 2012, Michael Kunz. All rights reserved.
//	http://kunzmi.github.io/managedCuda
//
//	This file is part of ManagedCuda.
//
//	ManagedCuda is free software: you can redistribute it and/or modify
//	it under the terms of the GNU Lesser General Public License as 
//	published by the Free Software Foundation, either version 2.1 of the 
//	License, or (at your option) any later version.
//
//	ManagedCuda is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//	GNU Lesser General Public License for more details.
//
//	You should have received a copy of the GNU Lesser General Public
//	License along with this library; if not, write to the Free Software
//	Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
//	MA 02110-1301  USA, http://www.gnu.org/licenses/.


using System;
using System.Collections;
using System.Collections.Generic;
using ManagedCuda.BasicTypes;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ManagedCuda
{
    /// <summary>
    /// A variable located in page locked (pinned) host memory. Use this type of variabe for asynchronous memcpy.
    /// </summary>
    /// <typeparam name="T">variable base type</typeparam>
    public class CudaPageLockedHostMemory3D<T> : IDisposable, IEnumerable<T> where T : struct
    {
        IntPtr _intPtr;
        SizeT _sizeInBytes = 0;
        SizeT _width = 0;
        SizeT _pitchInBytes = 0;
        SizeT _height = 0;
        SizeT _depth = 0;
        SizeT _typeSize = 0;
        CUResult res;
        bool disposed;

        #region Constructor
        /// <summary>
        /// Creates a new CudaPageLockedHostMemory3D and allocates the memory on host. Using cuMemHostAlloc
        /// </summary>
        /// <param name="width">In elements</param>
        /// <param name="pitchInBytes">Width including alignment in bytes</param>
        /// <param name="height">In elements</param>
        /// <param name="depth">In elements</param>
        /// <param name="allocFlags"></param>
        public CudaPageLockedHostMemory3D(SizeT width, SizeT pitchInBytes, SizeT height, SizeT depth, CUMemHostAllocFlags allocFlags)
        {
            _intPtr = new IntPtr();
            _width = width;
            _pitchInBytes = pitchInBytes;
            _height = height;
            _depth = depth;
            _typeSize = (SizeT)Marshal.SizeOf(typeof(T));
            _sizeInBytes = _pitchInBytes * _height * _depth;

            if (_typeSize * width > _pitchInBytes)
                throw new ArgumentException("pitchInBytes must be greater or equal to width * sizeof(T)", "pitchInBytes");

            res = DriverAPINativeMethods.MemoryManagement.cuMemHostAlloc(ref _intPtr, _sizeInBytes, allocFlags);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemHostAlloc", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Creates a new CudaPageLockedHostMemory3D and allocates the memory on host. Using cuMemHostAlloc without flags.
        /// </summary>
        /// <param name="width">In elements</param>
        /// <param name="pitchInBytes">Width including alignment in bytes</param>
        /// <param name="height">In elements</param>
        /// <param name="depth">In elements</param>
        public CudaPageLockedHostMemory3D(SizeT width, SizeT pitchInBytes, SizeT height, SizeT depth)
            : this(width, pitchInBytes, height, depth, 0)
        {

        }

        /// <summary>
        /// Creates a new CudaPageLockedHostMemory3D and allocates the memory on host. Using cuMemHostAlloc without flags.<para/>
        /// Pitch is assumed to be width * sizeof(T). Using cuMemHostAlloc without flags.
        /// </summary>
        /// <param name="width">In elements</param>
        /// <param name="height">In elements</param>
        /// <param name="depth">In elements</param>
        public CudaPageLockedHostMemory3D(SizeT width, SizeT height, SizeT depth)
            : this(width, width * (SizeT)Marshal.SizeOf(typeof(T)), height, depth, 0)
        {

        }

        /// <summary>
        /// Creates a new CudaPageLockedHostMemory3D and allocates the memory on host. Using cuMemHostAlloc without flags.<para/>
        /// Pitch is assumed to be width * sizeof(T). Using cuMemHostAlloc.
        /// </summary>
        /// <param name="width">In elements</param>
        /// <param name="height">In elements</param>
        /// <param name="depth">In elements</param>
        /// <param name="allocFlags"></param>
        public CudaPageLockedHostMemory3D(SizeT width, SizeT height, SizeT depth, CUMemHostAllocFlags allocFlags)
            : this(width, width * (SizeT)Marshal.SizeOf(typeof(T)), height, depth, allocFlags)
        {

        }

        /// <summary>
        /// For dispose
        /// </summary>
        ~CudaPageLockedHostMemory3D()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// For IDisposable
        /// </summary>
        /// <param name="fDisposing"></param>
        protected virtual void Dispose(bool fDisposing)
        {
            if (fDisposing && !disposed)
            {
                res = DriverAPINativeMethods.MemoryManagement.cuMemFreeHost(_intPtr);
                Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemFreeHost", res));
                disposed = true;
            }
            if (!fDisposing && !disposed)
                Debug.WriteLine(String.Format("ManagedCUDA not-disposed warning: {0}", this.GetType()));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Pointer to pinned host memory.
        /// </summary>
        public IntPtr PinnedHostPointer
        {
            get { return _intPtr; }
        }

        /// <summary>
        /// Width in elements
        /// </summary>
        public SizeT Width
        {
            get { return _width; }
        }

        /// <summary>
        /// Height in elements
        /// </summary>
        public SizeT Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Depth in elements
        /// </summary>
        public SizeT Depth
        {
            get { return _depth; }
        }

        /// <summary>
        /// Pitch in bytes
        /// </summary>
        public SizeT Pitch
        {
            get { return _pitchInBytes; }
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public SizeT SizeInBytes
        {
            get { return _sizeInBytes; }
        }

        /// <summary>
        /// Type size in bytes
        /// </summary>
        public SizeT TypeSize
        {
            get { return _typeSize; }
        }

        /// <summary>
        /// Access array per element.<para/>
        /// Each single access hast to trespass the managed/unmanged memory barrier. Access is therefor rather slow.
        /// </summary>
        /// <param name="x">X-index in elements</param>
        /// <param name="y">Y-index in elements</param>
        /// <param name="z">Z-index in elements</param>
        /// <returns></returns>
        public T this[SizeT x, SizeT y, SizeT z]
        {
            get 
            {
                SizeT index = _pitchInBytes * _height * z + _pitchInBytes * y + x * _typeSize;
				IntPtr position = new IntPtr((long)index + _intPtr.ToInt64());
                T ret = (T)Marshal.PtrToStructure(position, typeof(T));
                return ret;
            }
            set
            {
                SizeT index = _pitchInBytes * _height * z + _pitchInBytes * y + x * _typeSize;
				IntPtr position = new IntPtr((long)index + _intPtr.ToInt64());
                Marshal.StructureToPtr(value, position, false);
            }
        }
        #endregion

        #region Synchron Copy Methods
        #region DevicePtr
        /// <summary>
        /// Synchron copy host to device
        /// </summary>
        /// <param name="devicePtr"></param>
        public void SynchronCopyToDevice(CUdeviceptr devicePtr)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            res = DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyHtoD_v2(devicePtr, this._intPtr, SizeInBytes);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpyHtoD", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Synchron copy host to device
        /// </summary>
        /// <param name="devicePtr"></param>
        public void SynchronCopyToDevice(CudaDeviceVariable<T> devicePtr)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            res = DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyHtoD_v2(devicePtr.DevicePointer, this._intPtr, SizeInBytes);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpyHtoD", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Synchron copy device to host
        /// </summary>
        /// <param name="devicePtr"></param>
        public void SynchronCopyToHost(CUdeviceptr devicePtr)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            res = DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyDtoH_v2(this._intPtr, devicePtr, SizeInBytes);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpyDtoH", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Synchron copy device to host
        /// </summary>
        /// <param name="devicePtr"></param>
        public void SynchronCopyToHost(CudaDeviceVariable<T> devicePtr)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            res = DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpyDtoH_v2(this._intPtr, devicePtr.DevicePointer, SizeInBytes);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpyDtoH", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }
        #endregion

        #region Array3D
        /// <summary>
        /// Synchron copy host to 3D array
        /// </summary>
        /// <param name="deviceArray"></param>
        public void SynchronCopyToArray3D(CUarray deviceArray)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUDAMemCpy3D cpyProps = new CUDAMemCpy3D();
            cpyProps.dstArray = deviceArray;
            cpyProps.dstMemoryType = CUMemoryType.Array;
            cpyProps.srcHost = _intPtr;
            cpyProps.srcMemoryType = CUMemoryType.Host;
            cpyProps.srcPitch = _pitchInBytes;
            cpyProps.WidthInBytes = _width * _typeSize;
            cpyProps.Height = _height;
            cpyProps.Depth = _depth;

            res = DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpy3D_v2(ref cpyProps);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpy3D", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Synchron copy host to 3D Array
        /// </summary>
        /// <param name="array"></param>
        public void SynchronCopyToArray3D(CudaArray3D array)
        {
            SynchronCopyToArray3D(array.CUArray);
        }

        /// <summary>
        /// Synchron copy 3D Array to host
        /// </summary>
        /// <param name="deviceArray"></param>
        public void SynchronCopyFromArray3D(CUarray deviceArray)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUDAMemCpy3D cpyProps = new CUDAMemCpy3D();
            cpyProps.srcArray = deviceArray;
            cpyProps.srcMemoryType = CUMemoryType.Array;
            cpyProps.dstHost = _intPtr;
            cpyProps.dstMemoryType = CUMemoryType.Host;
            cpyProps.dstPitch = _pitchInBytes;
            cpyProps.WidthInBytes = _width * _typeSize;
            cpyProps.Height = _height;
            cpyProps.Depth = _depth;

            res = DriverAPINativeMethods.SynchronousMemcpy_v2.cuMemcpy3D_v2(ref cpyProps);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpy3D", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Synchron copy 3D Array to host
        /// </summary>
        /// <param name="array"></param>
        public void SynchronCopyFromArray3D(CudaArray3D array)
        {
            SynchronCopyFromArray3D(array.CUArray);
        }
        #endregion
        #endregion

        #region Asynchron Copy Methods
        #region DevicePtr
        /// <summary>
        /// Asynchron Copy host to device
        /// </summary>
        /// <param name="devicePtr"></param>
        /// <param name="stream"></param>
        public void AsyncCopyToDevice(CUdeviceptr devicePtr, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            res = DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyHtoDAsync_v2(devicePtr, _intPtr, SizeInBytes, stream);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpyHtoDAsync", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Asynchron Copy host to device
        /// </summary>
        /// <param name="deviceVar"></param>
        /// <param name="stream"></param>
        public void AsyncCopyToDevice(CudaDeviceVariable<T> deviceVar, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            res = DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyHtoDAsync_v2(deviceVar.DevicePointer, _intPtr, SizeInBytes, stream);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpyHtoDAsync", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }


        /// <summary>
        /// Asynchron copy device to host
        /// </summary>
        /// <param name="devicePtr"></param>
        /// <param name="stream"></param>
        public void AsyncCopyFromDevice(CUdeviceptr devicePtr, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            res = DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyDtoHAsync_v2(_intPtr, devicePtr, SizeInBytes, stream);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpyDtoHAsync", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Asynchron copy device to host
        /// </summary>
        /// <param name="deviceVar"></param>
        /// <param name="stream"></param>
        public void AsyncCopyFromDevice(CudaDeviceVariable<T> deviceVar, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            res = DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpyDtoHAsync_v2(_intPtr, deviceVar.DevicePointer, SizeInBytes, stream);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpyDtoHAsync", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }
        #endregion

        #region Array3D
        /// <summary>
        /// Asynchron copy host to 3D array
        /// </summary>
        /// <param name="deviceArray"></param>
        /// <param name="stream"></param>
        public void AsyncCopyToArray3D(CUarray deviceArray, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUDAMemCpy3D cpyProps = new CUDAMemCpy3D();
            cpyProps.dstArray = deviceArray;
            cpyProps.dstMemoryType = CUMemoryType.Array;
            cpyProps.srcHost = _intPtr;
            cpyProps.srcMemoryType = CUMemoryType.Host;
            cpyProps.srcPitch = _pitchInBytes;
            cpyProps.WidthInBytes = _width * _typeSize;
            cpyProps.Height = _height;
            cpyProps.Depth = _depth;

            res = DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpy3DAsync_v2(ref cpyProps, stream);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpy3DAsync", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Asynchron copy host to 3D Array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="stream"></param>
        public void AsyncCopyToArray3D(CudaArray3D array, CUstream stream)
        {
            AsyncCopyToArray3D(array.CUArray, stream);
        }

        /// <summary>
        /// Asynchron copy 3D Array to host
        /// </summary>
        /// <param name="deviceArray"></param>
        /// <param name="stream"></param>
        public void AsyncCopyFromArray3D(CUarray deviceArray, CUstream stream)
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUDAMemCpy3D cpyProps = new CUDAMemCpy3D();
            cpyProps.srcArray = deviceArray;
            cpyProps.srcMemoryType = CUMemoryType.Array;
            cpyProps.dstHost = _intPtr;
            cpyProps.dstMemoryType = CUMemoryType.Host;
            cpyProps.dstPitch = _pitchInBytes;
            cpyProps.WidthInBytes = _width * _typeSize;
            cpyProps.Height = _height;
            cpyProps.Depth = _depth;

            res = DriverAPINativeMethods.AsynchronousMemcpy_v2.cuMemcpy3DAsync_v2(ref cpyProps, stream);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemcpy3DAsync", res));
            if (res != CUResult.Success) throw new CudaException(res);
        }

        /// <summary>
        /// Asynchron copy 3D Array to host
        /// </summary>
        /// <param name="array"></param>
        /// <param name="stream"></param>
        public void AsyncCopyFromArray3D(CudaArray3D array, CUstream stream)
        {
            AsyncCopyFromArray3D(array.CUArray, stream);
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Returns the CUdeviceptr for pinned host memory mapped to device memory space. Only valid if context is created with flag <see cref="CUCtxFlags.MapHost"/>
        /// </summary>
        /// <returns>Device Pointer</returns>
        public CUdeviceptr GetDevicePointer()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUdeviceptr ptr = new CUdeviceptr();
            res = DriverAPINativeMethods.MemoryManagement.cuMemHostGetDevicePointer_v2(ref ptr, _intPtr, 0);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemHostGetDevicePointer", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return ptr;
        }

        /// <summary>
        /// Passes back the flags that were specified when allocating the pinned host buffer
        /// </summary>
        /// <returns></returns>
        public CUMemHostAllocFlags GetAllocFlags()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            CUMemHostAllocFlags flags = new CUMemHostAllocFlags();
            res = DriverAPINativeMethods.MemoryManagement.cuMemHostGetFlags(ref flags, _intPtr);
            Debug.WriteLine(String.Format("{0:G}, {1}: {2}", DateTime.Now, "cuMemHostGetDevicePointer", res));
            if (res != CUResult.Success) throw new CudaException(res);
            return flags;
        }
        #endregion

        #region IEnumerable
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            IEnumerator<T> enumerator = new CudaPageLockedHostMemory3DEnumerator<T>(this);
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (disposed) throw new ObjectDisposedException(this.ToString());
            IEnumerator enumerator = new CudaPageLockedHostMemory3DEnumerator<T>(this);
            return enumerator;
        }

        #endregion
    }

    /// <summary>
    /// Enumerator class for CudaPageLockedHostMemory3D
    /// </summary>
    /// <typeparam name="T_"></typeparam>
    public class CudaPageLockedHostMemory3DEnumerator<T_> : IEnumerator<T_> where T_ : struct
    {
        private CudaPageLockedHostMemory3D<T_> _memory = null;
        private SizeT _currentX = -1;
        private SizeT _currentY = 0;
        private SizeT _currentZ = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        public CudaPageLockedHostMemory3DEnumerator(CudaPageLockedHostMemory3D<T_> memory)
        {
            _memory = memory;
        }

        void IDisposable.Dispose() { }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            _currentX = -1;
            _currentY = 0;
            _currentZ = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public T_ Current
        {
            get { return _memory[_currentX, _currentY, _currentZ]; }
        }

        /// <summary>
        /// 
        /// </summary>
        object IEnumerator.Current
        {
            get { return _memory[_currentX, _currentY, _currentZ]; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            _currentX+=1;
			if ((long)_currentX >= (long)_memory.Width)
            {
                _currentX = 0;
                _currentY+=1;

				if ((long)_currentY >= (long)_memory.Height)
                {
                    _currentY = 0;
                    _currentZ+=1;
                }
            }

			if ((long)_currentZ >= (long)_memory.Depth)
                return false;
            else
                return true;
        }
    }
}
