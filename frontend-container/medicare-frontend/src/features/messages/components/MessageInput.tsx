import React, { useState } from "react";
import { Button } from "@shared/components";
import { Send } from "lucide-react";

import type { MessageInputProps } from "../types";

export const MessageInput: React.FC<MessageInputProps> = ({
  onSendMessage,
  isLoading = false,
  placeholder = "Type your message...",
}) => {
  const [message, setMessage] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (message.trim() && !isLoading) {
      onSendMessage(message.trim());
      setMessage("");
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e);
    }
  };

  return (
    <div className="bg-white border-t border-gray-200 p-4">
      <form onSubmit={handleSubmit} className="flex items-center gap-3">
        {/* Input field */}
        <div className="flex-1 relative">
          <textarea
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            onKeyPress={handleKeyPress}
            placeholder={placeholder}
            disabled={isLoading}
            rows={1}
            className="
              w-full px-4 py-0 border border-gray-300 rounded-lg
              resize-none focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
              disabled:bg-gray-100 disabled:cursor-not-allowed
              h-[50px] max-h-[120px] leading-[50px] align-middle
            "
            style={{
              height: "50px",
              lineHeight: "50px",
            }}
            onInput={(e) => {
              const target = e.target as HTMLTextAreaElement;
              target.style.height = "50px";
              const newHeight = Math.min(target.scrollHeight, 120);
              target.style.height = `${newHeight}px`;
              target.style.lineHeight = newHeight > 50 ? "1.5" : "50px";
            }}
          />
        </div>

        {/* Send button */}
        <Button
          type="submit"
          disabled={!message.trim() || isLoading}
          variant="primary"
          className="
            px-4 rounded-lg flex items-center justify-center gap-2
            disabled:opacity-50 disabled:cursor-not-allowed
            min-w-[80px] h-[50px] flex-shrink-0
          "
        >
          {isLoading ? (
            <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
          ) : (
            <>
              <Send size={18} />
              <span className="hidden sm:inline">Send</span>
            </>
          )}
        </Button>
      </form>
    </div>
  );
};
