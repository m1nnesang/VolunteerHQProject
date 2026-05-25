import { useCallback, useEffect, useRef, useState } from 'react'
import { useParams } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getMessages, sendMessage, updateMessage, deleteMessage, markMessageAsRead } from '../api/messages'
import { connectMessages } from '../api/realtime'

const PAGE_SIZE = 30

export default function MessagesPage() {
  const { userId: otherUserIdStr } = useParams()
  const otherUserId = parseInt(otherUserIdStr)
  const { user } = useAuth()

  const [messages, setMessages] = useState([])
  const [loading, setLoading] = useState(true)
  const [page, setPage] = useState(1)
  const [hasMore, setHasMore] = useState(false)
  const [loadingOlder, setLoadingOlder] = useState(false)
  const [text, setText] = useState('')
  const [sending, setSending] = useState(false)
  const [editingId, setEditingId] = useState(null)
  const [editText, setEditText] = useState('')

  const scrollRef = useRef(null)
  const bottomRef = useRef(null)
  const myId = user?.userId

  const markIncomingRead = useCallback((items) => {
    items
      .filter(m => !m.isRead && m.senderId === otherUserId)
      .forEach(m => markMessageAsRead(m.id).catch(() => {}))
  }, [otherUserId])

  useEffect(() => {
    let active = true
    setLoading(true)
    setPage(1)
    getMessages(otherUserId, 1, PAGE_SIZE)
      .then(res => {
        if (!active) return
        const items = (res.data?.items || []).slice().reverse()
        setMessages(items)
        setHasMore((res.data?.total || 0) > items.length)
        markIncomingRead(items)
        requestAnimationFrame(() => bottomRef.current?.scrollIntoView())
      })
      .catch(console.error)
      .finally(() => active && setLoading(false))
    return () => { active = false }
  }, [otherUserId, markIncomingRead])

  useEffect(() => {
    const conn = connectMessages((msg) => {
      const relevant =
        (msg.senderId === otherUserId && msg.receiverId === myId) ||
        (msg.senderId === myId && msg.receiverId === otherUserId)
      if (!relevant) return

      setMessages(prev => prev.some(m => m.id === msg.id) ? prev : [...prev, msg])

      if (msg.senderId === otherUserId) {
        markMessageAsRead(msg.id).catch(() => {})
        requestAnimationFrame(() => bottomRef.current?.scrollIntoView({ behavior: 'smooth' }))
      }
    })
    return () => conn?.stop?.()
  }, [otherUserId, myId])

  const loadOlder = async () => {
    if (loadingOlder || !hasMore) return
    setLoadingOlder(true)
    const container = scrollRef.current
    const prevHeight = container?.scrollHeight || 0
    try {
      const next = page + 1
      const res = await getMessages(otherUserId, next, PAGE_SIZE)
      const older = (res.data?.items || []).slice().reverse()
      setMessages(prev => [...older, ...prev])
      setPage(next)
      setHasMore((res.data?.total || 0) > (messages.length + older.length))
      requestAnimationFrame(() => {
        if (container) container.scrollTop = container.scrollHeight - prevHeight
      })
    } catch (err) {
      console.error(err)
    } finally {
      setLoadingOlder(false)
    }
  }

  const handleScroll = (e) => {
    if (e.target.scrollTop < 60 && hasMore && !loadingOlder) {
      loadOlder()
    }
  }

  const handleSend = async (e) => {
    e.preventDefault()
    if (!text.trim()) return
    setSending(true)
    try {
      const res = await sendMessage({ receiverId: otherUserId, text: text.trim() })
      setMessages(prev => prev.some(m => m.id === res.data.id) ? prev : [...prev, res.data])
      setText('')
      requestAnimationFrame(() => bottomRef.current?.scrollIntoView({ behavior: 'smooth' }))
    } catch (err) {
      console.error(err)
    } finally {
      setSending(false)
    }
  }

  const handleEdit = async (id) => {
    if (!editText.trim()) return
    try {
      const res = await updateMessage(id, { text: editText.trim() })
      setMessages(prev => prev.map(m => m.id === id ? res.data : m))
      setEditingId(null)
    } catch (err) {
      console.error(err)
    }
  }

  const handleDelete = async (id) => {
    try {
      await deleteMessage(id)
      setMessages(prev => prev.filter(m => m.id !== id))
    } catch (err) {
      console.error(err)
    }
  }

  return (
    <div className="min-h-screen bg-[#111218] flex flex-col">
      <div className="max-w-2xl w-full mx-auto px-4 py-6 flex flex-col flex-1">

        {/* Header */}
        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl px-5 py-4 mb-4 flex items-center gap-3">
          <div className="w-9 h-9 bg-yellow-400/20 rounded-full flex items-center justify-center text-yellow-400 text-sm font-bold shrink-0">
            #{otherUserId}
          </div>
          <div>
            <div className="text-white text-sm font-medium">Користувач #{otherUserId}</div>
            <div className="text-gray-600 text-xs">Особисті повідомлення</div>
          </div>
        </div>

        {/* Messages */}
        <div
          ref={scrollRef}
          onScroll={handleScroll}
          className="flex-1 bg-[#1c1d24] border border-[#2e303a] rounded-xl p-4 overflow-y-auto space-y-3 min-h-[400px] max-h-[60vh]"
        >
          {loadingOlder && (
            <div className="text-center text-gray-600 text-xs py-2">Завантаження історії...</div>
          )}
          {loading ? (
            <div className="text-center text-gray-500 text-sm pt-10">Завантаження...</div>
          ) : messages.length === 0 ? (
            <div className="text-center text-gray-600 text-sm pt-10">Повідомлень поки немає. Напишіть першим!</div>
          ) : (
            messages.map(m => {
              const isMine = m.senderId === myId
              return (
                <div key={m.id} className={`flex ${isMine ? 'justify-end' : 'justify-start'}`}>
                  <div className={`max-w-[75%] group relative`}>
                    {editingId === m.id ? (
                      <div className="flex gap-2">
                        <input
                          value={editText}
                          onChange={e => setEditText(e.target.value)}
                          onKeyDown={e => e.key === 'Enter' && handleEdit(m.id)}
                          autoFocus
                          className="bg-[#111218] border border-yellow-400 rounded-lg px-3 py-2 text-white text-sm focus:outline-none"
                        />
                        <button onClick={() => handleEdit(m.id)} className="text-yellow-400 text-xs">✓</button>
                        <button onClick={() => setEditingId(null)} className="text-gray-500 text-xs">✕</button>
                      </div>
                    ) : (
                      <>
                        <div className={`px-4 py-2.5 rounded-2xl text-sm leading-relaxed ${
                          isMine
                            ? 'bg-yellow-400 text-black rounded-br-sm'
                            : 'bg-[#2a2b35] text-gray-200 rounded-bl-sm'
                        }`}>
                          {m.text}
                          {m.isEdited && <span className="text-xs opacity-50 ml-1">(ред.)</span>}
                        </div>
                        <div className={`text-xs text-gray-600 mt-1 ${isMine ? 'text-right' : ''}`}>
                          {new Date(m.sentAt).toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' })}
                          {isMine && <span className="ml-1">{m.isRead ? '✓✓' : '✓'}</span>}
                        </div>
                        {isMine && (
                          <div className="absolute -top-6 right-0 hidden group-hover:flex items-center gap-1 bg-[#1c1d24] border border-[#2e303a] rounded-lg px-2 py-1">
                            <button
                              onClick={() => { setEditingId(m.id); setEditText(m.text) }}
                              className="text-gray-400 hover:text-yellow-400 text-xs transition-colors"
                            >
                              ✏️
                            </button>
                            <button
                              onClick={() => handleDelete(m.id)}
                              className="text-gray-400 hover:text-red-400 text-xs transition-colors"
                            >
                              🗑
                            </button>
                          </div>
                        )}
                      </>
                    )}
                  </div>
                </div>
              )
            })
          )}
          <div ref={bottomRef} />
        </div>

        {/* Input */}
        <form onSubmit={handleSend} className="mt-4 flex gap-2">
          <input
            value={text}
            onChange={e => setText(e.target.value)}
            placeholder="Напишіть повідомлення..."
            className="flex-1 bg-[#1c1d24] border border-[#2e303a] rounded-xl px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors"
          />
          <button
            type="submit"
            disabled={sending || !text.trim()}
            className="bg-yellow-400 text-black font-semibold px-5 py-3 rounded-xl hover:bg-yellow-300 transition-colors disabled:opacity-50"
          >
            →
          </button>
        </form>
      </div>
    </div>
  )
}
