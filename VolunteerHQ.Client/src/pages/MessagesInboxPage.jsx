import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getConversations } from '../api/messages'

export default function MessagesInboxPage() {
  const [conversations, setConversations] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getConversations()
      .then(res => setConversations(res.data || []))
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [])

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-2xl mx-auto px-4 py-10">
        <h1 className="text-white text-2xl font-bold mb-6">Повідомлення</h1>

        {loading ? (
          <div className="text-gray-500 text-sm">Завантаження...</div>
        ) : conversations.length === 0 ? (
          <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-12 text-center text-gray-600 text-sm">
            У вас поки немає діалогів
          </div>
        ) : (
          <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl overflow-hidden">
            {conversations.map(c => (
              <Link
                key={c.otherUserId}
                to={`/messages/${c.otherUserId}`}
                className="flex items-center gap-4 px-5 py-4 border-b border-[#2e303a] last:border-0 hover:bg-[#111218] transition-colors"
              >
                <div className="w-10 h-10 bg-yellow-400/20 rounded-full flex items-center justify-center text-yellow-400 text-sm font-bold shrink-0">
                  #{c.otherUserId}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between gap-2">
                    <span className="text-white text-sm font-medium">Користувач #{c.otherUserId}</span>
                    <span className="text-gray-600 text-xs shrink-0">
                      {new Date(c.lastMessageAt).toLocaleString('uk-UA', {
                        day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit'
                      })}
                    </span>
                  </div>
                  <div className="flex items-center justify-between gap-2 mt-0.5">
                    <p className="text-gray-500 text-sm truncate">{c.lastMessage}</p>
                    {c.unreadCount > 0 && (
                      <span className="bg-yellow-400 text-black text-xs font-bold px-1.5 rounded-full shrink-0">
                        {c.unreadCount}
                      </span>
                    )}
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
