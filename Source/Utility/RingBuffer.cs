using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZippyBackup
{
	public class RingBuffer : IDisposable
	{	
		protected byte[]	m_Buffer;
		protected int		m_nAlloc;

			// Buffer is empty if these values are equal.
		protected int		m_iHead;			// Data is read from the head, and then removed from the ring.
		protected int		m_iTail;			// Data is added at the tail, and the length increases.
        
        #region "Initialization / Cleanup"

        public RingBuffer(int Size = 4096)
        {
            m_Buffer = new byte [Size];
            m_nAlloc = Size;
            m_iHead = m_iTail = 0;
        }

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return; 

            if (disposing) {
                // Free managed...
                m_Buffer = null;
                m_nAlloc = 0;
                m_iTail = m_iHead = 0;
            }

            // Free unmanaged...
            disposed = true;
        }

        public void Dispose()
        { 
            Dispose(true);
            GC.SuppressFinalize(this);           
        }	    

        #endregion
        
		public void Clear() { m_iHead = m_iTail = 0; }
		public bool IsEmpty { get { return (m_iHead == m_iTail); } }
		public bool IsFull { get { return Length + 1 >= m_nAlloc; } }
        /// <summary>
        /// Retrieves the number of bytes available for additional storage in the buffer without
        /// discarding any existing data.
        /// </summary>
        public int CapacityAvailable
        {
            get
            {
                if (m_nAlloc == 0) return 0;
			    // The +1 and -1 are to accomodate the ring buffer's requirement that the
			    // head == tail case uses up one byte otherwise available.		        
		        return m_nAlloc - Length - 1; 
            }
        }
        /// <summary>Retrieves the number of bytes currently stored.  That is, bytes which can be retrieved by 
		/// calls to Get().</summary>
		public int Length
        {
            get
            {
		        if (m_iTail < m_iHead) return (m_nAlloc - m_iHead) + m_iTail;		// Wraps-around
		        else return m_iTail - m_iHead;										// Linear or empty
            }
        }

		public void Enqueue(byte b)
        {
            if (CapacityAvailable < 1) throw new InsufficientMemoryException("Insufficient space in buffer to support this operation.");
            m_Buffer[m_iTail] = b;
            m_iTail ++;	            
            if (m_iTail >= m_nAlloc) m_iTail = 0;
        }

		public void Enqueue(byte[] buffer, int offset, int count)
        {
            if (CapacityAvailable < count) throw new InsufficientMemoryException("Insufficient space in buffer to support this operation.");
            
			if (m_iTail < m_iHead){		// Wrap-around
				
                Buffer.BlockCopy(buffer, offset, m_Buffer, m_iTail, count);
				m_iTail += count;
				if (m_iTail >= m_nAlloc) m_iTail = 0;
			}
			else {							// Linear or empty

				int FirstLen = Math.Min(count, m_nAlloc - m_iTail);
                Buffer.BlockCopy(buffer, offset, m_Buffer, m_iTail, FirstLen);
				m_iTail += FirstLen;
				if (m_iTail >= m_nAlloc) m_iTail = 0;

				if (FirstLen < count){

					// At this point, we must now be wrap-around style.  This is because
					// the (m_iTail >= m_nAlloc) statement MUST have been true, since 
					// for FirstLen to be anything other than exactly equal to count, 
					// the MIN would have had to have chosen m_nAlloc - m_iTail, and so:
					//		m_iTail += FirstLen;
					//		m_iTail += (m_nAlloc - m_iTail);
					//		m_iTail = m_iTail + GetAlloc() - m_iTail;
					//		m_iTail = m_nAlloc;
					//		Then, m_iTail -> 0;
					// Finally, m_iHead cannot be equal to zero because MakeSpace() has
					// assured us that this data will fit properly.

                    Buffer.BlockCopy(buffer, offset + FirstLen, m_Buffer, m_iTail, count - FirstLen);					
					m_iTail += (count - FirstLen);
					if (m_iTail >= m_nAlloc) m_iTail = 0;
				}
			}		
        }

		public byte Dequeue()
        {
		    if (m_iHead == m_iTail) throw new InvalidOperationException("No data available in queue.");
	
		    byte b = m_Buffer[m_iHead];
            m_iHead ++;		
		    if (m_iHead >= m_nAlloc) m_iHead = 0;
            return b;
        }        

		public void Dequeue(byte[] buffer, int offset, int count)
        {
            if (Length < count) throw new InvalidOperationException("Insufficient data available in queue.");
		    
			if (m_iTail < m_iHead)		// Wrap-around
            {
				int FirstLen = Math.Min(count, (m_nAlloc - m_iHead));
				Buffer.BlockCopy(m_Buffer, m_iHead, buffer, offset, FirstLen);
				m_iHead += FirstLen;
				if (m_iHead >= m_nAlloc) m_iHead = 0;

				if (FirstLen < count)
                {
					// At this point, we must now be linear.  This is because
					// the (m_iHead >= m_nAlloc) statement MUST have been true, since 
					// for FirstLen to be anything other than exactly equal to count, 
					// the MIN would have had to have chosen m_nAlloc - m_iHead, and so:
					//		m_iHead += count;
					//		m_iHead += (m_nAlloc - m_iHead);
					//		m_iHead = m_iHead + m_nAlloc - m_iHead;
					//		m_iHead = m_nAlloc;
					//		Then, m_iHead = 0;
					// Finally, m_iTail cannot be equal to zero because we've already
					// checked that this data is available.

					Buffer.BlockCopy(m_Buffer, m_iHead, buffer, offset + FirstLen, count - FirstLen);
					m_iHead += (count - FirstLen);
					if (m_iHead >= m_nAlloc) m_iHead = 0;
				}
			}
			else {							// Linear				
                Buffer.BlockCopy(m_Buffer, m_iHead, buffer, offset, count);				
				m_iHead += count;
				if (m_iHead >= m_nAlloc) m_iHead = 0;
			}
        }

		void Discard(int count)
        {
            if (Length < count) throw new InvalidOperationException("Insufficient data available in queue.");
		    
			if (m_iTail < m_iHead)		// Wrap-around
            {
				int FirstLen = Math.Min(count, (m_nAlloc - m_iHead));				
				m_iHead += FirstLen;
				if (m_iHead >= m_nAlloc) m_iHead = 0;

				if (FirstLen < count)
                {
					// At this point, we must now be linear.  This is because
					// the (m_iHead >= m_nAlloc) statement MUST have been true, since 
					// for FirstLen to be anything other than exactly equal to count, 
					// the MIN would have had to have chosen m_nAlloc - m_iHead, and so:
					//		m_iHead += count;
					//		m_iHead += (m_nAlloc - m_iHead);
					//		m_iHead = m_iHead + m_nAlloc - m_iHead;
					//		m_iHead = m_nAlloc;
					//		Then, m_iHead = 0;
					// Finally, m_iTail cannot be equal to zero because we've already
					// checked that this data is available.
					
					m_iHead += (count - FirstLen);
					if (m_iHead >= m_nAlloc) m_iHead = 0;
				}
			}
			else {							// Linear				                
				m_iHead += count;
				if (m_iHead >= m_nAlloc) m_iHead = 0;
			}
        }
	};		
}
